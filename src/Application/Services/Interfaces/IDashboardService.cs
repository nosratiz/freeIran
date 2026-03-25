using Application.DTOs;
using FluentResults;

namespace Application.Services.Interfaces;

/// <summary>
/// Dashboard service interface for dependency injection.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Retrieves aggregated dashboard statistics.
    /// </summary>
    Task<Result<DashboardStatsResponse>> GetStatsAsync(CancellationToken cancellationToken = default);
}
