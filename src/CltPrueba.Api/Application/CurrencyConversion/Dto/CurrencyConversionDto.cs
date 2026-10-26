namespace CltPrueba.Api.Application.CurrencyConversion.Dto;

public record CurrencyConversionDto(string FromCurrency, string ToCurrency, decimal OriginalAmount, decimal ConvertedAmount);
