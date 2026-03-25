using System.Text.Json;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Infrastructure.Constants;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Configuration;

/// <summary>
/// Extension methods for integrating AWS Secrets Manager with the configuration system.
/// Retrieves sensitive configuration at application startup.
/// </summary>
public static class SecretsManagerSetup
{
    /// <summary>
    /// Adds AWS Secrets Manager as a configuration source.
    /// Retrieves secrets and adds them to the configuration.
    /// </summary>
    public static IConfigurationBuilder AddAwsSecretsManager(
        this IConfigurationBuilder builder,
        string secretName,
        string region)
    {
        var configuration = builder.Build();
        
        // Allow override for local development
        var useLocalSecrets = configuration.GetValue<bool>(DynamoDbConstants.ConfigurationKeys.UseLocalSecrets);
        
        if (useLocalSecrets)
        {
            // In development, use local secrets or environment variables
            return builder;
        }

        var secretsJson = GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        
        if (!string.IsNullOrEmpty(secretsJson))
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(secretsJson));
            builder.AddJsonStream(stream);
        }

        return builder;
    }

    /// <summary>
    /// Retrieves a secret value from AWS Secrets Manager.
    /// </summary>
    private static async Task<string?> GetSecretAsync(string secretName, string region)
    {
        using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
        
        try
        {
            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = DynamoDbConstants.SecretsManager.VersionStage
            };

            var response = await client.GetSecretValueAsync(request);

            return response.SecretString;
        }
        catch (ResourceNotFoundException)
        {
            throw new InvalidOperationException(
                DynamoDbConstants.ErrorMessages.SecretNotFound(secretName));
        }
        catch (InvalidRequestException ex)
        {
            throw new InvalidOperationException(
                DynamoDbConstants.ErrorMessages.InvalidRequest(ex.Message), ex);
        }
        catch (InvalidParameterException ex)
        {
            throw new InvalidOperationException(
                DynamoDbConstants.ErrorMessages.InvalidParameter(ex.Message), ex);
        }
    }
}