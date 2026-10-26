using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Application.Users.Dto;
using CltPrueba.Api.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Users.Commands;

public class UpdateUserHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public UpdateUserHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<UserDto>> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);
        if (user is null)
        {
            return Result<UserDto>.NotFound("Usuario no encontrado");
        }

        user.Name = command.Name.Trim();
        user.Email = command.Email.Trim().ToLowerInvariant();
        user.IsActive = command.IsActive!.Value;
        
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        { 
            return Result<UserDto>.Conflict($"Ya existe un usuario con el email {command.Email}");
        }
        return Result<UserDto>.Ok(new UserDto(user.Id, user.Name, user.Email, user.IsActive));
    }
}