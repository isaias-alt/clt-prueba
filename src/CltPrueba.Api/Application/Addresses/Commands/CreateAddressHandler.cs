using CltPrueba.Api.Application.Addresses.Dto;
using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Domain;
using CltPrueba.Api.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Addresses.Commands;

public class CreateAddressHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CreateAddressHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<AddressDto>> HandleAsync(CreateAddressCommand command, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var userExists = await db.Users.AnyAsync(u => u.Id == command.UserId, cancellationToken);
        if (!userExists)
        {
            return Result<AddressDto>.NotFound("Usuario no encontrado");
        }

        var address = new Address
        {
            UserId = command.UserId,
            Street = command.Street.Trim(),
            City = command.City.Trim(),
            Country = command.Country.Trim(),
            ZipCode = string.IsNullOrWhiteSpace(command.ZipCode) ? null : command.ZipCode.Trim()
        };
        db.Addresses.Add(address);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            return Result<AddressDto>.NotFound("Usuario no encontrado");
        }

        return Result<AddressDto>.Ok(new AddressDto(address.Id, address.UserId, address.Street, address.City, address.Country, address.ZipCode));
    }
}
