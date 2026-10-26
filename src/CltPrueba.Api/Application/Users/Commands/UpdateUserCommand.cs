namespace CltPrueba.Api.Application.Users.Commands;

public record UpdateUserCommand(int Id, string Name, string Email, bool? IsActive);