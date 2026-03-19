using FluentValidation;
using MyApp.Common;
using MyApp.Services.DataContracts.Order;
using MyApp.Services.Interfaces;

namespace MyApp.Endpoints;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll)
            .WithName("GetAllOrders")
            .WithSummary("Returns a paginated list of orders")
            .Produces<PagedResult<OrderResponseListItem>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetOrderById")
            .WithSummary("Returns a single order by ID")
            .Produces<OrderResponseDetail>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateOrder")
            .WithSummary("Creates a new order")
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPatch("/{id:guid}/status", UpdateStatus)
            .WithName("UpdateOrderStatus")
            .WithSummary("Updates the status of an order")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteOrder")
            .WithSummary("Deletes an order")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAll(
        IOrderService service,
        CancellationToken ct,
        int page = 1,
        int pageSize = 20) =>
        TypedResults.Ok(await service.GetAllAsync(page, pageSize, ct));

    private static async Task<IResult> GetById(
        Guid id, IOrderService service, CancellationToken ct) =>
        await service.GetByIdAsync(id, ct) is { } order
            ? TypedResults.Ok(order)
            : TypedResults.NotFound();

    private static async Task<IResult> Create(
        OrderRequestCreate request,
        IValidator<OrderRequestCreate> validator,
        IOrderService service,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        var result = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/v1/orders/{result.Id}", result);
    }

    private static async Task<IResult> UpdateStatus(
        Guid id,
        OrderRequestUpdateStatus request,
        IValidator<OrderRequestUpdateStatus> validator,
        IOrderService service,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return TypedResults.ValidationProblem(validation.ToDictionary());

        return await service.UpdateStatusAsync(id, request, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<IResult> Delete(
        Guid id, IOrderService service, CancellationToken ct) =>
        await service.DeleteAsync(id, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
}
