using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Endpoints;
using MyApp.Infrastructure.DbContext;
using MyApp.Services.DependencyInjection;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine($"[Serilog] {msg}"));

    builder.Host.UseSerilog((context, config) =>
        config.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddOpenApi();
    builder.Services.AddProblemDetails();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    builder.Services.AddProductServices();
    builder.Services.AddOrderServices();

    var app = builder.Build();

    app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;
        var (status, title) = ex switch
        {
            MyApp.Common.NotFoundException e => (404, e.Message),
            MyApp.Common.ConflictException e => (409, e.Message),
            _ => (500, "An unexpected error occurred")
        };
        await Results.Problem(title: title, statusCode: status).ExecuteAsync(context);
    }));

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.MapGroup("/api/v1/products")
        .MapProductEndpoints()
        .WithTags("Products");

    app.MapGroup("/api/v1/orders")
        .MapOrderEndpoints()
        .WithTags("Orders");

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
