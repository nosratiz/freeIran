using Application.Constants;
using Application.DTOs;
using Domain.Aggregates;
using Domain.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Application service for generating dashboard statistics.
/// Groups data in-memory since DynamoDB doesn't support native GROUP BY.
/// </summary>
public sealed partial class DashboardService : IDashboardService
{
    private readonly IRegistrantRepository _repository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IRegistrantRepository repository,
        ILogger<DashboardService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    #region High-Performance Logging (Serilog Source Generators)

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "Fetching dashboard statistics")]
    private partial void LogFetchingStats();

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Error,
        Message = "Failed to fetch registrants for dashboard")]
    private partial void LogFetchFailed();

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Debug,
        Message = "Processing {RegistrantCount} registrants for dashboard stats")]
    private partial void LogProcessingRegistrants(int registrantCount);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Information,
        Message = "Dashboard stats generated: {TotalRegistrations} total registrations, {CountryCount} countries")]
    private partial void LogStatsGenerated(int totalRegistrations, int countryCount);

    #endregion

    /// <summary>
    /// Retrieves aggregated dashboard statistics.
    /// Groups registrants by Country, Gender, Age Group, and Education Level.
    /// </summary>
    public async Task<Result<DashboardStatsResponse>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        LogFetchingStats();

        // Fetch all registrants (in production, consider caching or pre-aggregation)
        var fetchResult = await _repository.GetAllAsync(cancellationToken);

        if (fetchResult.IsFailed)
        {
            LogFetchFailed();
            return Result.Fail<DashboardStatsResponse>(fetchResult.Errors);
        }

        var registrants = fetchResult.Value;

        LogProcessingRegistrants(registrants.Count);

        // Group by Country
        var byCountry = registrants
            .GroupBy(r => r.Profile.Country)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by Gender
        var byGender = registrants
            .GroupBy(r => r.Profile.Gender)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        // Group by Age Group (calculated from DateOfBirth)
        var byAgeGroup = registrants
            .GroupBy(r => r.GetAgeGroup())
            .ToDictionary(g => DisplayStrings.AgeGroups.Get(g.Key), g => g.Count());

        // Group by Education Level
        var byEducation = registrants
            .GroupBy(r => r.Profile.EducationLevel)
            .ToDictionary(g => DisplayStrings.EducationLevels.Get(g.Key), g => g.Count());

        // Business Contribution Stats
        var canRunBusiness = registrants.Count(r => r.Contribution.CanRunBusiness);
        var canDonate = registrants.Count(r => r.Contribution.CanDonate);
        var both = registrants.Count(r => r.Contribution.CanRunBusiness && r.Contribution.CanDonate);

        var response = new DashboardStatsResponse
        {
            TotalRegistrations = registrants.Count,
            ByCountry = byCountry,
            ByGender = byGender,
            ByAgeGroup = byAgeGroup,
            ByEducationLevel = byEducation,
            BusinessContributions = new BusinessStats(canRunBusiness, canDonate, both),
            GeneratedAtUtc = DateTime.UtcNow
        };

        LogStatsGenerated(response.TotalRegistrations, byCountry.Count);

        return Result.Ok(response);
    }
}

/// <summary>
/// Dashboard service interface for dependency injection.
/// </summary>
public interface IDashboardService
{
    Task<Result<DashboardStatsResponse>> GetStatsAsync(CancellationToken cancellationToken = default);
}
