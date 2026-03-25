namespace Application.DTOs;

/// <summary>
/// Response DTO for successful registration.
/// </summary>
public sealed record RegisterResponse(
    string Id,
    string Name,
    string Email,
    DateTime CreatedAtUtc);
