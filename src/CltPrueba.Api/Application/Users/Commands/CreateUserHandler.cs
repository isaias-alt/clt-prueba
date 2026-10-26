using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Application.Users.Dto;
using CltPrueba.Api.Domain;
using CltPrueba.Api.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Users.Commands;

public class CreateUserHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly PasswordHasher<User> _hasher = new();
    
    public CreateUserHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<UserDto>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Name = command.Name.Trim(),
            Email = command.Email.Trim().ToLowerInvariant(),
        };

        if (command.Password is not null)
        {
            user.PasswordHash = _hasher.HashPassword(user, command.Password);
        }
        
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            return Result<UserDto>.Conflict($"Ya existe un usuario con el email {command.Email}");
        }
        
        return Result<UserDto>.Ok(new UserDto( user.Id, user.Name, user.Email, user.IsActive ));
    }
}