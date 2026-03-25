namespace Infrastructure.Configuration;

/// <summary>
/// Configuration class for AWS Secrets Manager integration.
/// </summary>
public sealed class AwsSecretsManagerSettings
{
    public const string SectionName = "AwsSecretsManager";
    
    public required string SecretName { get; init; }
    public required string Region { get; init; }
}