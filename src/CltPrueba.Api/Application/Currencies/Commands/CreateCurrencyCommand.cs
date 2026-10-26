namespace CltPrueba.Api.Application.Currencies.Commands;

public record CreateCurrencyCommand(string Code, string Name, decimal RateToBase);
