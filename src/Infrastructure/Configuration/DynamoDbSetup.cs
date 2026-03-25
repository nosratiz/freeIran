using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Domain.Repositories;
using Infrastructure.Constants;
using Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring DynamoDB services.
/// </summary>
public static class DynamoDbSetup
{
    /// <summary>
    /// Adds DynamoDB services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddDynamoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind settings from configuration
        services.Configure<DynamoDbSettings>(
            configuration.GetSection(DynamoDbSettings.SectionName));

        var settings = configuration
            .GetSection(DynamoDbSettings.SectionName)
            .Get<DynamoDbSettings>();

        if (settings is null)
        {
            throw new InvalidOperationException(
                DynamoDbConstants.ErrorMessages.MissingSettings(DynamoDbSettings.SectionName));
        }

        // Configure the DynamoDB client
        var clientConfig = new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(settings.Region)
        };

        // Use service URL for local development (e.g., DynamoDB Local)
        if (!string.IsNullOrEmpty(settings.ServiceUrl))
        {
            clientConfig.ServiceURL = settings.ServiceUrl;
        }

        // Check if we have explicit credentials from secrets
        var accessKey = configuration[DynamoDbConstants.ConfigurationKeys.AccessKey];
        var secretKey = configuration[DynamoDbConstants.ConfigurationKeys.SecretKey];

        IAmazonDynamoDB dynamoDbClient;
        
        if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
        {
            // Use explicit credentials (from Secrets Manager)
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            dynamoDbClient = new AmazonDynamoDBClient(credentials, clientConfig);
        }
        else
        {
            // Use default credential chain (IAM role, environment, etc.)
            dynamoDbClient = new AmazonDynamoDBClient(clientConfig);
        }

        services.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
        services.AddSingleton<IDynamoDBContext>(_ => new DynamoDBContext(dynamoDbClient));

        // Register repository
        services.AddScoped<IRegistrantRepository, DynamoDbRegistrantRepository>();

        return services;
    }
}
