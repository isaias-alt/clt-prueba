namespace CltPrueba.Api.Application.Users.Dto;

public record BulkUserFailureDto(int Index, string? Email, string Reason);
