namespace CltPrueba.Api.Application.Users.Commands;

public record BulkCreateUsersCommand(List<CreateUserCommand?> Users);
