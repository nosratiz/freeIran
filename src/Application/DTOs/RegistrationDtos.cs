using Domain.Aggregates;

namespace Application.DTOs;

/// <summary>
/// Request DTO for registrant registration.
/// </summary>
public sealed record RegisterRequest
{
    // Profile Information
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string Country { get; init; }
    public required Gender Gender { get; init; }
    public required EducationLevel EducationLevel { get; init; }

    // Skills Information
    public required IReadOnlyList<string> LanguagesSpoken { get; init; }
    public required IReadOnlyList<string> ProfessionalSkills { get; init; }

    // Business Contribution
    public required bool CanRunBusiness { get; init; }
    public required bool CanDonate { get; init; }
}

/// <summary>
/// Response DTO for successful registration.
/// </summary>
public sealed record RegisterResponse(
    string Id,
    string Name,
    string Email,
    DateTime CreatedAtUtc);

/// <summary>
/// Dashboard statistics response DTO.
/// </summary>
public sealed record DashboardStatsResponse
{
    public required int TotalRegistrations { get; init; }
    public required IReadOnlyDictionary<string, int> ByCountry { get; init; }
    public required IReadOnlyDictionary<string, int> ByGender { get; init; }
    public required IReadOnlyDictionary<string, int> ByAgeGroup { get; init; }
    public required IReadOnlyDictionary<string, int> ByEducationLevel { get; init; }
    public required BusinessStats BusinessContributions { get; init; }
    public required DateTime GeneratedAtUtc { get; init; }
}

public sealed record BusinessStats(
    int CanRunBusinessCount,
    int CanDonateCount,
    int BothCount);

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

public sealed record ValidationError(string Field, string Message);
