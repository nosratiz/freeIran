using Domain.Aggregates;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Constants;
using Infrastructure.Persistence;

namespace Infrastructure.Mappers;

/// <summary>
/// Mapper for converting between domain Registrant and RegistrantEntity for DynamoDB persistence.
/// </summary>
public static class RegistrantEntityMapper
{
    /// <summary>
    /// Creates a DynamoDB entity from a domain registrant.
    /// </summary>
    public static RegistrantEntity ToEntity(this Registrant registrant)
    {
        return new RegistrantEntity
        {
            Id = registrant.Id,
            Email = registrant.Profile.Email,
            Name = registrant.Profile.Name,
            DateOfBirth = registrant.Profile.DateOfBirth.ToString(DynamoDbConstants.DateFormats.DateOnly),
            Country = registrant.Profile.Country,
            Gender = registrant.Profile.Gender.ToString(),
            EducationLevel = registrant.Profile.EducationLevel.ToString(),
            LanguagesSpoken = registrant.Skills.LanguagesSpoken.ToList(),
            ProfessionalSkills = registrant.Skills.ProfessionalSkills.ToList(),
            CanRunBusiness = registrant.Contribution.CanRunBusiness,
            CanDonate = registrant.Contribution.CanDonate,
            CreatedAtUtc = registrant.CreatedAtUtc.ToString(DynamoDbConstants.DateFormats.RoundTrip),
            UpdatedAtUtc = registrant.UpdatedAtUtc?.ToString(DynamoDbConstants.DateFormats.RoundTrip)
        };
    }

    /// <summary>
    /// Converts the DynamoDB entity back to a domain registrant.
    /// </summary>
    public static Registrant ToDomain(this RegistrantEntity entity)
    {
        return new Registrant
        {
            Id = entity.Id,
            Profile = new RegistrantProfile(
                entity.Name,
                entity.Email,
                DateOnly.ParseExact(entity.DateOfBirth, DynamoDbConstants.DateFormats.DateOnly),
                entity.Country,
                Enum.Parse<Gender>(entity.Gender),
                Enum.Parse<EducationLevel>(entity.EducationLevel)),
            Skills = new SkillsProfile(
                entity.LanguagesSpoken,
                entity.ProfessionalSkills),
            Contribution = new BusinessContribution(
                entity.CanRunBusiness,
                entity.CanDonate),
            CreatedAtUtc = DateTime.Parse(entity.CreatedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            UpdatedAtUtc = string.IsNullOrEmpty(entity.UpdatedAtUtc) 
                ? null 
                : DateTime.Parse(entity.UpdatedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }
}
