namespace CltPrueba.Api.Application.Users.Dto;

public record BulkCreateUsersResultDto(
    int Total,
    int CreatedCount,
    int FailedCount,
    List<UserDto> Created,
    List<BulkUserFailureDto> Failed);
