using CltPrueba.Api.Application.Addresses.Commands;
using CltPrueba.Api.Application.Addresses.Dto;
using CltPrueba.Api.Application.Addresses.Queries;
using CltPrueba.Api.Application.Common;
using FluentValidation;

namespace CltPrueba.Api.Api;

public static class AddressesEndpoints
{
    public static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        var userAddresses = app.MapGroup("/users/{userId:int}/addresses").WithTags("Addresses");

        userAddresses.MapPost("/", CreateAddress);
        userAddresses.MapGet("/", ListAddressesByUser);
        
        var addresses = app.MapGroup("/addresses").WithTags("Addresses");

        addresses.MapPut("/{id:int}", UpdateAddress);
        addresses.MapDelete("/{id:int}", DeleteAddress);

        return app;
    }

    private static async Task<IResult> CreateAddress(
        int userId,
        AddressRequest request,
        IValidator<CreateAddressCommand> validator,
        CreateAddressHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateAddressCommand(userId, request.Street, request.City, request.Country, request.ZipCode);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await handler.HandleAsync(command, cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok => Results.Created($"/users/{userId}/addresses", result.Value),
            ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
            _ => Results.Problem()
        };
    }

    private static async Task<IResult> ListAddressesByUser(
        int userId,
        ListAddressesByUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ListAddressesByUserQuery(userId), cancellationToken);
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }
        
        return Results.NotFound(new { error = result.Error });
    }

    private static async Task<IResult> UpdateAddress(
        int id,
        AddressRequest request,
        IValidator<UpdateAddressCommand> validator,
        UpdateAddressHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAddressCommand(id, request.Street, request.City, request.Country, request.ZipCode);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await handler.HandleAsync(command, cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok => Results.Ok(result.Value),
            ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
            _ => Results.Problem()
        };
    }

    private static async Task<IResult> DeleteAddress(int id, DeleteAddressHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteAddressCommand(id), cancellationToken);
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }
        
        return Results.NotFound(new { error = result.Error });
    }
}
