using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Application.CurrencyConversion.Dto;
using CltPrueba.Api.Domain;
using CltPrueba.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.CurrencyConversion;

public class ConvertCurrencyHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ConvertCurrencyHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<CurrencyConversionDto>> HandleAsync(ConvertCurrencyQuery query, CancellationToken cancellationToken)
    {
        var fromCode = query.FromCurrencyCode.Trim().ToUpperInvariant();
        var toCode = query.ToCurrencyCode.Trim().ToUpperInvariant();
        
        var fromTask = FindByCodeAsync(fromCode, cancellationToken);
        var toTask = FindByCodeAsync(toCode, cancellationToken);

        await Task.WhenAll(fromTask, toTask);

        var fromCurrency = await fromTask;
        var toCurrency = await toTask;

        if (fromCurrency is null || toCurrency is null)
        {
            var missing = new List<string>();
            if (fromCurrency is null)
            {
                missing.Add(fromCode);
            }
            if (toCurrency is null)
            {
                missing.Add(toCode);
            }

            return Result<CurrencyConversionDto>.NotFound($"Moneda no encontrada: {string.Join(", ", missing.Distinct())}");
        }

        var baseAmount = query.Amount * fromCurrency.RateToBase;
        var convertedAmount = Math.Round(baseAmount / toCurrency.RateToBase, 6, MidpointRounding.AwayFromZero);

        return Result<CurrencyConversionDto>.Ok(
            new CurrencyConversionDto(fromCurrency.Code, toCurrency.Code, query.Amount, convertedAmount));
    }
    
    private async Task<Currency?> FindByCodeAsync(string code, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        return await db.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }
}
