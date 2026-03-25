using Application.Constants;
using Application.DTOs;
using Application.Mappers;
using Application.Services.Interfaces;
using Domain.Aggregates;
using Domain.Errors;
using Domain.Repositories;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Application service for handling registrant registration operations.
/// Uses FluentResults for control flow instead of exceptions.
/// </summary>
public sealed partial class RegistrationService : IRegistrationService
{
    private readonly IRegistrantRepository _repository;
    private readonly IValidator<RegisterRequest> _validator;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(
        IRegistrantRepository repository,
        IValidator<RegisterRequest> validator,
        ILogger<RegistrationService> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    #region High-Performance Logging (Serilog Source Generators)

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Registration validation failed for {Email} with {ErrorCount} errors")]
    private partial void LogValidationFailed(string email, int errorCount);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Failed to check email existence for {Email}")]
    private partial void LogEmailCheckFailed(string email);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Registration attempt with existing email: {Email}")]
    private partial void LogDuplicateEmail(string email);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "Failed to create registrant for {Email}")]
    private partial void LogCreateFailed(string email);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Information,
        Message = "Successfully registered new participant {RegistrantId} with email {Email}")]
    private partial void LogRegistrationSuccess(string registrantId, string email);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Warning,
        Message = "Failed to retrieve registrant with ID {RegistrantId}")]
    private partial void LogRetrieveFailed(string registrantId);

    #endregion

    /// <summary>
    /// Registers a new participant in the portal.
    /// </summary>
    public async Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Validate the request using FluentValidation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            LogValidationFailed(request.Email, validationResult.Errors.Count);

            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage)
                    .WithMetadata("Field", e.PropertyName)
                    .WithMetadata("Code", DomainErrors.Registrant.ValidationFailed))
                .ToList();

            return Result.Fail<RegisterResponse>(errors);
        }

        // Step 2: Check if email already exists
        var existsResult = await _repository.ExistsByEmailAsync(request.Email, cancellationToken);
        
        if (existsResult.IsFailed)
        {
            LogEmailCheckFailed(request.Email);
            return Result.Fail<RegisterResponse>(existsResult.Errors);
        }

        if (existsResult.Value)
        {
            LogDuplicateEmail(request.Email);
            return Result.Fail<RegisterResponse>(
                new Error(ErrorMessages.Email.AlreadyExists)
                    .WithMetadata("Code", DomainErrors.Registrant.EmailAlreadyExists)
                    .WithMetadata("Field", nameof(request.Email)));
        }

        // Step 3: Create the domain entity
        var registrant = request.ToRegistrant();

        // Step 4: Persist to repository
        var createResult = await _repository.CreateAsync(registrant, cancellationToken);
        
        if (createResult.IsFailed)
        {
            LogCreateFailed(request.Email);
            return Result.Fail<RegisterResponse>(createResult.Errors);
        }

        LogRegistrationSuccess(createResult.Value.Id, createResult.Value.Profile.Email);

        // Step 5: Return success response
        return Result.Ok(createResult.Value.ToResponse());
    }

    /// <summary>
    /// Retrieves a registrant by their ID.
    /// </summary>
    public async Task<Result<Registrant>> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result.Fail<Registrant>(
                new Error(ErrorMessages.Registrant.IdRequired)
                    .WithMetadata("Code", DomainErrors.Registrant.ValidationFailed));
        }

        var result = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (result.IsFailed)
        {
            LogRetrieveFailed(id);
        }

        return result;
    }
}
