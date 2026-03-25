using Infrastructure.Constants;

namespace Infrastructure.Settings;

/// <summary>
/// DynamoDB configuration settings.
/// </summary>
public sealed class DynamoDbSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = DynamoDbConstants.ConfigurationSections.DynamoDb;
    
    /// <summary>
    /// DynamoDB table name for registrants.
    /// </summary>
    public required string TableName { get; init; }
    
    /// <summary>
    /// Service URL for local DynamoDB development.
    /// </summary>
    public required string ServiceUrl { get; init; }
    
    /// <summary>
    /// AWS region for DynamoDB.
    /// </summary>
    public required string Region { get; init; }
}
