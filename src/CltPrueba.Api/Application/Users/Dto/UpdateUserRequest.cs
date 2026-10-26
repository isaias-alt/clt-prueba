namespace CltPrueba.Api.Application.Users.Dto;

public record UpdateUserRequest(string Name, string Email, bool? IsActive);