using Domain.Constants;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Aggregates;

/// <summary>
/// Aggregate root representing a portal registrant.
/// Combines all profile information for DynamoDB storage.
/// </summary>
public sealed record Registrant
{
    public required string Id { get; init; }
    public required RegistrantProfile Profile { get; init; }
    public required SkillsProfile Skills { get; init; }
    public required BusinessContribution Contribution { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>
    /// Calculates the age group based on date of birth.
    /// </summary>
    public AgeGroup GetAgeGroup()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - Profile.DateOfBirth.Year;
        
        if (Profile.DateOfBirth > today.AddYears(-age))
            age--;

        return age switch
        {
            <= AgeGroupBoundaries.Under18Max => AgeGroup.Under18,
            <= AgeGroupBoundaries.Age18To24Max => AgeGroup.Age18To24,
            <= AgeGroupBoundaries.Age25To34Max => AgeGroup.Age25To34,
            <= AgeGroupBoundaries.Age35To44Max => AgeGroup.Age35To44,
            <= AgeGroupBoundaries.Age45To54Max => AgeGroup.Age45To54,
            <= AgeGroupBoundaries.Age55To64Max => AgeGroup.Age55To64,
            _ => AgeGroup.Age65Plus
        };
    }
}
