using Amazon.DynamoDBv2.DataModel;
using Domain.Aggregates;
using Infrastructure.Constants;

namespace Infrastructure.Persistence;

/// <summary>
/// DynamoDB entity for storing registrant data.
/// Maps to/from the domain Registrant aggregate.
/// </summary>
[DynamoDBTable(DynamoDbConstants.Tables.Registrants)]
public sealed class RegistrantEntity
{
    [DynamoDBHashKey]
    public string Id { get; set; } = string.Empty;

    // Profile fields (flattened for DynamoDB)
    [DynamoDBGlobalSecondaryIndexHashKey(DynamoDbConstants.Indexes.EmailIndex)]
    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    
    public string DateOfBirth { get; set; } = string.Empty;
    
    [DynamoDBGlobalSecondaryIndexHashKey(DynamoDbConstants.Indexes.CountryIndex)]
    public string Country { get; set; } = string.Empty;
    
    public string Gender { get; set; } = string.Empty;
    
    public string EducationLevel { get; set; } = string.Empty;

    // Skills fields
    public List<string> LanguagesSpoken { get; set; } = [];
    
    public List<string> ProfessionalSkills { get; set; } = [];

    // Business Contribution fields
    public bool CanRunBusiness { get; set; }
    
    public bool CanDonate { get; set; }

    // Metadata
    public string CreatedAtUtc { get; set; } = string.Empty;
    
    public string? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Creates a DynamoDB entity from a domain registrant.
    /// </summary>
    public static RegistrantEntity FromDomain(Registrant registrant)
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
    public Registrant ToDomain()
    {
        return new Registrant
        {
            Id = Id,
            Profile = new RegistrantProfile(
                Name,
                Email,
                DateOnly.ParseExact(DateOfBirth, DynamoDbConstants.DateFormats.DateOnly),
                Country,
                Enum.Parse<Gender>(Gender),
                Enum.Parse<EducationLevel>(EducationLevel)),
            Skills = new SkillsProfile(
                LanguagesSpoken,
                ProfessionalSkills),
            Contribution = new BusinessContribution(
                CanRunBusiness,
                CanDonate),
            CreatedAtUtc = DateTime.Parse(CreatedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
            UpdatedAtUtc = string.IsNullOrEmpty(UpdatedAtUtc) 
                ? null 
                : DateTime.Parse(UpdatedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind)
        };
    }
}
