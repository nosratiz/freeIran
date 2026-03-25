namespace Infrastructure.Configuration;

/// <summary>
/// Structure of secrets stored in AWS Secrets Manager.
/// This defines what secrets the application expects.
/// </summary>
public sealed class AppSecrets
{
    public string? DynamoDbAccessKey { get; init; }
    public string? DynamoDbSecretKey { get; init; }
    public string? KmsKeyArn { get; init; }
    public string? JwtSigningKey { get; init; }
    public string? ApiRateLimitKey { get; init; }
}