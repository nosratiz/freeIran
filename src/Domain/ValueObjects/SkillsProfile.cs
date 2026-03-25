namespace Domain.ValueObjects;

/// <summary>
/// Value object representing the registrant's skills and languages.
/// </summary>
public sealed record SkillsProfile(
    IReadOnlyList<string> LanguagesSpoken,
    IReadOnlyList<string> ProfessionalSkills);
