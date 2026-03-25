using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories;
using FluentResults;
using Infrastructure.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence;

/// <summary>
/// DynamoDB implementation of the registrant repository.
/// </summary>
public sealed partial class DynamoDbRegistrantRepository : IRegistrantRepository
{
    private readonly IDynamoDBContext _context;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly DynamoDbSettings _settings;
    private readonly ILogger<DynamoDbRegistrantRepository> _logger;

    public DynamoDbRegistrantRepository(
        IDynamoDBContext context,
        IAmazonDynamoDB dynamoDbClient,
        IOptions<DynamoDbSettings> settings,
        ILogger<DynamoDbRegistrantRepository> logger)
    {
        _context = context;
        _dynamoDbClient = dynamoDbClient;
        _settings = settings.Value;
        _logger = logger;
    }

    #region High-Performance Logging (Serilog Source Generators)

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "Created registrant {RegistrantId} in DynamoDB table {TableName}")]
    private partial void LogRegistrantCreated(string registrantId, string tableName);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Error,
        Message = "DynamoDB error creating registrant {RegistrantId}: {ErrorMessage}")]
    private partial void LogDynamoDbCreateError(Exception ex, string registrantId, string errorMessage);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Error,
        Message = "Unexpected error creating registrant {RegistrantId}")]
    private partial void LogUnexpectedCreateError(Exception ex, string registrantId);

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Error,
        Message = "DynamoDB error fetching registrant {RegistrantId}: {ErrorMessage}")]
    private partial void LogDynamoDbFetchError(Exception ex, string registrantId, string errorMessage);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Error,
        Message = "DynamoDB error fetching registrant by email: {ErrorMessage}")]
    private partial void LogDynamoDbEmailFetchError(Exception ex, string errorMessage);

    [LoggerMessage(
        EventId = 3006,
        Level = LogLevel.Debug,
        Message = "Retrieved {RegistrantCount} registrants from DynamoDB table {TableName}")]
    private partial void LogRegistrantsRetrieved(int registrantCount, string tableName);

    [LoggerMessage(
        EventId = 3007,
        Level = LogLevel.Error,
        Message = "DynamoDB error scanning registrants: {ErrorMessage}")]
    private partial void LogDynamoDbScanError(Exception ex, string errorMessage);

    #endregion

    public async Task<Result<Registrant>> CreateAsync(
        Registrant registrant,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = RegistrantEntity.FromDomain(registrant);
            
            await _context.SaveAsync(entity, new DynamoDBOperationConfig
            {
                OverrideTableName = _settings.TableName
            }, cancellationToken);

            LogRegistrantCreated(registrant.Id, _settings.TableName);
            
            return Result.Ok(registrant);
        }
        catch (AmazonDynamoDBException ex)
        {
            LogDynamoDbCreateError(ex, registrant.Id, ex.Message);
            return Result.Fail<Registrant>(
                new Error(DynamoDbConstants.ErrorMessages.DatabaseError(ex.Message))
                    .WithMetadata("Code", DomainErrors.Repository.OperationFailed));
        }
        catch (Exception ex)
        {
            LogUnexpectedCreateError(ex, registrant.Id);
            return Result.Fail<Registrant>(
                new Error(DynamoDbConstants.ErrorMessages.UnexpectedError)
                    .WithMetadata("Code", DomainErrors.Repository.OperationFailed));
        }
    }

    public async Task<Result<Registrant>> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.LoadAsync<RegistrantEntity>(id, new DynamoDBOperationConfig
            {
                OverrideTableName = _settings.TableName
            }, cancellationToken);

            if (entity is null)
            {
                return Result.Fail<Registrant>(
                    new Error(DynamoDbConstants.ErrorMessages.RegistrantNotFound(id))
                        .WithMetadata("Code", DomainErrors.Registrant.NotFound));
            }

            return Result.Ok(entity.ToDomain());
        }
        catch (AmazonDynamoDBException ex)
        {
            LogDynamoDbFetchError(ex, id, ex.Message);
            return Result.Fail<Registrant>(
                new Error(DynamoDbConstants.ErrorMessages.DatabaseError(ex.Message))
                    .WithMetadata("Code", DomainErrors.Repository.OperationFailed));
        }
    }

    public async Task<Result<Registrant?>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            
            // Query using the GSI on email
            var queryConfig = new QueryOperationConfig
            {
                IndexName = DynamoDbConstants.Indexes.EmailIndex,
                KeyExpression = new Expression
                {
                    ExpressionStatement = DynamoDbConstants.KeyExpressions.EmailEquals,
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { DynamoDbConstants.ExpressionAttributes.EmailPlaceholder, normalizedEmail }
                    }
                },
                Limit = 1
            };

            var table = Table.LoadTable(_dynamoDbClient, _settings.TableName);
            var search = table.Query(queryConfig);
            var documents = await search.GetNextSetAsync(cancellationToken);

            if (documents.Count == 0)
            {
                return Result.Ok<Registrant?>(null);
            }

            var entity = _context.FromDocument<RegistrantEntity>(documents[0]);
            return Result.Ok<Registrant?>(entity.ToDomain());
        }
        catch (AmazonDynamoDBException ex)
        {
            LogDynamoDbEmailFetchError(ex, ex.Message);
            return Result.Fail<Registrant?>(
                new Error(DynamoDbConstants.ErrorMessages.DatabaseError(ex.Message))
                    .WithMetadata("Code", DomainErrors.Repository.OperationFailed));
        }
    }

    public async Task<Result<bool>> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var result = await GetByEmailAsync(email, cancellationToken);
        
        if (result.IsFailed)
        {
            return Result.Fail<bool>(result.Errors);
        }

        return Result.Ok(result.Value is not null);
    }

    public async Task<Result<IReadOnlyList<Registrant>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: In production, use pagination for large datasets
            var conditions = new List<ScanCondition>();
            
            var entities = await _context.ScanAsync<RegistrantEntity>(
                conditions,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _settings.TableName
                }).GetRemainingAsync(cancellationToken);

            var registrants = entities.Select(e => e.ToDomain()).ToList();
            
            LogRegistrantsRetrieved(registrants.Count, _settings.TableName);
            
            return Result.Ok<IReadOnlyList<Registrant>>(registrants);
        }
        catch (AmazonDynamoDBException ex)
        {
            LogDynamoDbScanError(ex, ex.Message);
            return Result.Fail<IReadOnlyList<Registrant>>(
                new Error(DynamoDbConstants.ErrorMessages.DatabaseError(ex.Message))
                    .WithMetadata("Code", DomainErrors.Repository.OperationFailed));
        }
    }
}

/// <summary>
/// DynamoDB configuration settings.
/// </summary>
public sealed class DynamoDbSettings
{
    public const string SectionName = DynamoDbConstants.ConfigurationSections.DynamoDb;
    
    public required string TableName { get; init; }
    public required string ServiceUrl { get; init; }
    public required string Region { get; init; }
}
