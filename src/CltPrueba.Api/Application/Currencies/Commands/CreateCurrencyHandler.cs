using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Application.Currencies.Dto;
using CltPrueba.Api.Domain;
using CltPrueba.Api.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Currencies.Commands;

public class CreateCurrencyHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateCurrencyHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<CurrencyDto>> HandleAsync(CreateCurrencyCommand command, CancellationToken cancellationToken)
    {
        var currency = new Currency
        {
            Code = command.Code.Trim().ToUpperInvariant(),
            Name = command.Name.Trim(),
            RateToBase = command.RateToBase
        };

        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        db.Currencies.Add(currency);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            return Result<CurrencyDto>.Conflict($"Ya existe una moneda con el codigo {currency.Code}");
        }

        return Result<CurrencyDto>.Ok(new CurrencyDto(currency.Id, currency.Code, currency.Name, currency.RateToBase));
    }
}
