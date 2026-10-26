namespace CltPrueba.Api.Application.Common;

public enum ResultStatus { Ok, NotFound, Conflict }

public record Result<T>(ResultStatus Status, T? Value = default, string? Error = null)
{
    public bool IsSuccess => Status == ResultStatus.Ok;
    
    public static Result<T> Ok(T value) => new(ResultStatus.Ok, value);
    public static Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, error);
    public static Result<T> Conflict(string error) => new(ResultStatus.Conflict, default, error);
}