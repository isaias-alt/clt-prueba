using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Application.Users.Commands;
using CltPrueba.Api.Application.Users.Dto;
using CltPrueba.Api.Application.Users.Queries;
using FluentValidation;

namespace CltPrueba.Api.Api;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapPost("/", CreateUser);
        group.MapPost("/bulk", BulkCreateUsers);
        group.MapGet("/", ListUsers);
        group.MapGet("/{id:int}", GetUserById);
        group.MapPut("/{id:int}", UpdateUser);
        group.MapDelete("/{id:int}", DeleteUser);
        
        return app;
    }

    private static async Task<IResult> CreateUser(
        CreateUserCommand command,
        IValidator<CreateUserCommand> validator,
        CreateUserHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await handler.HandleAsync(command, cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok => Results.Created($"/users/{result.Value!.Id}", result.Value),
            ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
            _ => Results.Problem()
        };
    }

    private static async Task<IResult> ListUsers(bool? isActive, ListUsersHandler handler,
        CancellationToken cancellationToken)
    {
        var users = await handler.HandleAsync(new ListUsersQuery(isActive), cancellationToken);
        return Results.Ok(users);
    }

    private static async Task<IResult> GetUserById(
        int id, 
        GetUserByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetUserByIdQuery(id), cancellationToken);
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }
        return Results.NotFound(new { error = result.Error });
    }
    
    private static async Task<IResult> UpdateUser(
        int id,
        UpdateUserRequest request,
        IValidator<UpdateUserCommand> validator,
        UpdateUserHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(id, request.Name, request.Email, request.IsActive);
        
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
            ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
            _ => Results.Problem()
        };
    }

    private static async Task<IResult> DeleteUser(int id, DeleteUserHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteUserCommand(id), cancellationToken);
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }
        
        return Results.NotFound(new {  error = result.Error });
        
    }

    private static async Task<IResult> BulkCreateUsers(
        BulkCreateUsersCommand command,
        IValidator<BulkCreateUsersCommand> validator,
        BulkCreateUsersHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await handler.HandleAsync(command, cancellationToken);
        return Results.Ok(result);
    }
}