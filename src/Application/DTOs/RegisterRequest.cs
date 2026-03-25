using Domain.Enums;

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
