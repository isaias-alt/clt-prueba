namespace CltPrueba.Api.Application.CurrencyConversion;

public record ConvertCurrencyQuery(string FromCurrencyCode, string ToCurrencyCode, decimal Amount);
