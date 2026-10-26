using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Addresses.Commands;

public class DeleteAddressHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteAddressHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<bool>> HandleAsync(DeleteAddressCommand command, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var deleted = await db.Addresses
            .Where(a => a.Id == command.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
        {
            return Result<bool>.NotFound("Direccion no encontrada");
        }
        
        return Result<bool>.Ok(true);
    }
}
