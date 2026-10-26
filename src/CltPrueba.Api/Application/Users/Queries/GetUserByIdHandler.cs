using CltPrueba.Api.Application.Common;
using CltPrueba.Api.Application.Users.Dto;
using CltPrueba.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Users.Queries;

public class GetUserByIdHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    
    public GetUserByIdHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Result<UserDto>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == query.Id)
            .Select(u => new UserDto(u.Id, u.Name, u.Email, u.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result<UserDto>.NotFound("Usuario no encontrado");
        }
        else
        {
            return Result<UserDto>.Ok(user);
        }
    }
}