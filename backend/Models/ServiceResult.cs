namespace OnlineCV.Api.Models;

public enum ServiceResultStatus
{
    Success,
    ValidationFailed,
    NotFound,
    Conflict
}

public sealed record ServiceError(string Code, string Message, string Field = "general");

public sealed class ServiceResult<T>
{
    private ServiceResult(ServiceResultStatus status, T? value, IReadOnlyList<ServiceError> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public ServiceResultStatus Status { get; }
    public T? Value { get; }
    public IReadOnlyList<ServiceError> Errors { get; }
    public bool IsSuccess => Status == ServiceResultStatus.Success;

    public static ServiceResult<T> Success(T value) =>
        new(ServiceResultStatus.Success, value, []);

    public static ServiceResult<T> Validation(IEnumerable<ServiceError> errors) =>
        new(ServiceResultStatus.ValidationFailed, default, errors.ToArray());

    public static ServiceResult<T> NotFound(string message) =>
        new(ServiceResultStatus.NotFound, default, [new ServiceError("not_found", message)]);

    public static ServiceResult<T> Conflict(string message, string field = "general") =>
        new(ServiceResultStatus.Conflict, default, [new ServiceError("conflict", message, field)]);
}
