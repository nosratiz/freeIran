using Domain.Enums;

namespace Domain.ValueObjects;

/// <summary>
/// Value object representing the registrant's basic profile information.
/// </summary>
public sealed record RegistrantProfile(
    string Name,
    string Email,
    DateOnly DateOfBirth,
    string Country,
    Gender Gender,
    EducationLevel EducationLevel);
