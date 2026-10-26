using CltPrueba.Api.Application.Currencies.Dto;
using CltPrueba.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Currencies.Queries;

public class ListCurrenciesHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ListCurrenciesHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<CurrencyDto>> HandleAsync(ListCurrenciesQuery query, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        return await db.Currencies
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CurrencyDto(c.Id, c.Code, c.Name, c.RateToBase))
            .ToListAsync(cancellationToken);
    }
}
