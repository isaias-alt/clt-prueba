using CltPrueba.Api.Application.Users.Dto;
using CltPrueba.Api.Application.Users.Queries;
using CltPrueba.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CltPrueba.Api.Application.Users.Queries;

public class ListUsersHandler
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    
    public ListUsersHandler(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<UserDto>> HandleAsync(ListUsersQuery query, CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);

        var users = db.Users.AsNoTracking();

        if (query.IsActive.HasValue)
        {
           users = users.Where(u => u.IsActive == query.IsActive.Value);
        }
        
        return await users
            .OrderBy(u => u.Id)
            .Select(u => new UserDto(u.Id, u.Name, u.Email, u.IsActive))
            .ToListAsync(cancellationToken);
    }
}