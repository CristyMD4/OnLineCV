using OnlineCV.Api.Models;

namespace OnlineCV.Api.Endpoints;

internal static class ApiResults
{
    public static IResult From<T>(ServiceResult<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value!);
        }

        return result.Status switch
        {
            ServiceResultStatus.ValidationFailed => Results.ValidationProblem(
                result.Errors
                    .GroupBy(error => error.Field)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.Message).Distinct().ToArray()),
                title: "One or more business rules were not satisfied."),
            ServiceResultStatus.NotFound => Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: result.Errors[0].Message),
            ServiceResultStatus.Conflict => Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Resource conflict",
                detail: result.Errors[0].Message,
                extensions: new Dictionary<string, object?> { ["field"] = result.Errors[0].Field }),
            _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
