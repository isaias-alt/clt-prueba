using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Application.Currencies.Commands;
using CltPrueba.Api.Application.Currencies.Queries;
using CltPrueba.Api.Application.CurrencyConversion;
using FluentValidation;

namespace CltPrueba.Api.Api;

public static class CurrenciesEndpoints
{
    public static IEndpointRouteBuilder MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        var currencies = app.MapGroup("/currencies").WithTags("Currencies");

        currencies.MapGet("/", ListCurrencies);
        currencies.MapPost("/", CreateCurrency);
        
        app.MapPost("/currency/convert", ConvertCurrency).WithTags("Currency");

        return app;
    }

    private static async Task<IResult> ListCurrencies(ListCurrenciesHandler handler, CancellationToken cancellationToken)
    {
        var currencies = await handler.HandleAsync(new ListCurrenciesQuery(), cancellationToken);
        return Results.Ok(currencies);
    }

    private static async Task<IResult> CreateCurrency(
        CreateCurrencyCommand command,
        IValidator<CreateCurrencyCommand> validator,
        CreateCurrencyHandler handler,
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
            ResultStatus.Ok => Results.Created("/currencies", result.Value),
            ResultStatus.Conflict => Results.Conflict(new { error = result.Error }),
            _ => Results.Problem()
        };
    }

    private static async Task<IResult> ConvertCurrency(
        ConvertCurrencyQuery query,
        IValidator<ConvertCurrencyQuery> validator,
        ConvertCurrencyHandler handler,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        var result = await handler.HandleAsync(query, cancellationToken);
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }
        
        return Results.NotFound(new { error = result.Error });
    }
}
