using Api.Constants;
using Application.Constants;
using Application.DTOs;
using FluentResults;

namespace Api.Endpoints;

/// <summary>
/// Extension methods for converting FluentResults to HTTP results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a FluentResults Result to an IResult for Minimal API responses.
    /// </summary>
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, IResult> onSuccess,
        Func<IError, int>? statusCodeOnFailure = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return ToErrorResult(result.Errors, statusCodeOnFailure);
    }

    /// <summary>
    /// Converts a list of FluentResults errors to an API error response.
    /// </summary>
    private static IResult ToErrorResult(
        List<IError> errors,
        Func<IError, int>? statusCodeOnFailure)
    {
        var primaryError = errors.FirstOrDefault();
        var statusCode = statusCodeOnFailure?.Invoke(primaryError!) ?? StatusCodes.Status400BadRequest;

        // Extract validation errors if present
        var validationErrors = errors
            .Where(e => e.HasMetadataKey("Field"))
            .Select(e => new ValidationError(
                e.Metadata["Field"]?.ToString() ?? ErrorMessages.General.UnknownField,
                e.Message))
            .ToList();

        var errorCode = primaryError?.Metadata.TryGetValue("Code", out var code) == true
            ? code?.ToString() ?? ApiConstants.ErrorCodes.UnknownError
            : ApiConstants.ErrorCodes.UnknownError;

        var response = new ApiErrorResponse
        {
            Code = errorCode,
            Message = primaryError?.Message ?? ErrorMessages.General.UnknownError,
            ValidationErrors = validationErrors.Count > 0 ? validationErrors : null,
            Timestamp = DateTime.UtcNow
        };

        return Results.Json(response, statusCode: statusCode);
    }
}
