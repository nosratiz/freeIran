using Amazon.DynamoDBv2.DataModel;
using Infrastructure.Constants;

namespace Infrastructure.Persistence;

/// <summary>
/// DynamoDB entity for storing registrant data.
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
}
