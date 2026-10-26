using CltPrueba.Api.Application.Addresses.Dto;
using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Addresses.Queries;

public class ListAddressesByUserHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ListAddressesByUserHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<List<AddressDto>>> HandleAsync(ListAddressesByUserQuery query, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var userExists = await db.Users.AnyAsync(u => u.Id == query.UserId, cancellationToken);
        if (!userExists)
        {
            return Result<List<AddressDto>>.NotFound("Usuario no encontrado");
        }

        var addresses = await db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == query.UserId)
            .OrderBy(a => a.Id)
            .Select(a => new AddressDto(a.Id, a.UserId, a.Street, a.City, a.Country, a.ZipCode))
            .ToListAsync(cancellationToken);

        return Result<List<AddressDto>>.Ok(addresses);
    }
}
