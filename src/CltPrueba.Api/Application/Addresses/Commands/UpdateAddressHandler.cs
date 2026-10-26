using CltPrueba.Api.Application.Addresses.Dto;
using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Addresses.Commands;

public class UpdateAddressHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UpdateAddressHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<AddressDto>> HandleAsync(UpdateAddressCommand command, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var address = await db.Addresses.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken);
        if (address is null)
        {
            return Result<AddressDto>.NotFound("Direccion no encontrada");
        }

        address.Street = command.Street.Trim();
        address.City = command.City.Trim();
        address.Country = command.Country.Trim();
        address.ZipCode = string.IsNullOrWhiteSpace(command.ZipCode) ? null : command.ZipCode.Trim();

        await db.SaveChangesAsync(cancellationToken);

        return Result<AddressDto>.Ok(new AddressDto(address.Id, address.UserId, address.Street, address.City, address.Country, address.ZipCode));
    }
}
