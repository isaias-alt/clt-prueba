using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Users.Commands;

public class DeleteUserHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DeleteUserHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<bool>> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var deleted = await db.Users
            .Where(u => u.Id == command.Id)
            .ExecuteDeleteAsync(cancellationToken);
        
        if (deleted == 0)
        {
            return Result<bool>.NotFound("Usuario no encontrado");
        }
        else
        {
            return Result<bool>.Ok(true);
        }
    }
}