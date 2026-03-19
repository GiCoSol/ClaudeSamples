using FluentValidation;
using MyApp.Common;
using MyApp.Services.DataContracts.Product;
using MyApp.Services.Interfaces;

namespace MyApp.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll)
            .WithName("GetAllProducts")
            .WithSummary("Returns a paginated list of products")
            .Produces<PagedResult<ProductResponseListItem>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetProductById")
            .WithSummary("Returns a single product by ID")
            .Produces<ProductResponseDetail>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateProduct")
            .WithSummary("Creates a new product")
            .Produces<ProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateProduct")
            .WithSummary("Updates an existing product")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteProduct")
            .WithSummary("Deletes a product")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(
        IProductService service,
        CancellationToken ct,
        int page = 1,
        int pageSize = 20) =>
        TypedResults.Ok(await service.GetAllAsync(page, pageSize, ct));

    private static async Task<IResult> GetById(
        Guid id, IProductService service, CancellationToken ct) =>
        await service.GetByIdAsync(id, ct) is { } product
            ? TypedResults.Ok(product)
            : TypedResults.NotFound();

    private static async Task<IResult> Create(
        ProductRequestCreate request,
        IValidator<ProductRequestCreate> validator,
        IProductService service,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var result = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/v1/products/{result.Id}", result);
    }

    private static async Task<IResult> Update(
        Guid id,
        ProductRequestUpdate request,
        IValidator<ProductRequestUpdate> validator,
        IProductService service,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        return await service.UpdateAsync(id, request, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<IResult> Delete(
        Guid id, IProductService service, CancellationToken ct) =>
        await service.DeleteAsync(id, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
}
