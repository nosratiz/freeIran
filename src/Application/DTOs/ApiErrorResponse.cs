namespace Application.DTOs;

/// <summary>
/// Standard API error response.
/// </summary>
public sealed record ApiErrorResponse
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public IReadOnlyList<ValidationError>? ValidationErrors { get; init; }
    public required DateTime Timestamp { get; init; }
}

/// <summary>
/// Validation error details.
/// </summary>
public sealed record ValidationError(string Field, string Message);
