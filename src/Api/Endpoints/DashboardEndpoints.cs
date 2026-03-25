using Api.Constants;
using Application.DTOs;
using Application.Services.Interfaces;

namespace Api.Endpoints;

/// <summary>
/// Minimal API endpoint definitions for the dashboard.
/// </summary>
public static class DashboardEndpoints
{
    /// <summary>
    /// Maps all dashboard-related endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{ApiConstants.Routes.ApiPrefix}{ApiConstants.Routes.Dashboard}")
            .WithTags(ApiConstants.Tags.Dashboard)
            .WithOpenApi();

        // GET /api/dashboard/stats
        group.MapGet(ApiConstants.Routes.DashboardStats, GetStatsAsync)
            .WithName("GetDashboardStats")
            .WithSummary("Get dashboard statistics")
            .WithDescription("Returns aggregate dashboard data including registrations grouped by Country, Gender, Age Group, and Education Level.")
            .Produces<DashboardStatsResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError)
            .RequireRateLimiting(ApiConstants.Policies.FixedRateLimiter)
            .CacheOutput(policy => policy
                .Expire(TimeSpan.FromMinutes(ApiConstants.CacheDefaults.DashboardStatsCacheMinutes))
                .Tag(ApiConstants.Policies.DashboardStatsCache));

        return app;
    }

    /// <summary>
    /// Handles GET /api/dashboard/stats
    /// </summary>
    private static async Task<IResult> GetStatsAsync(
        IDashboardService dashboardService,
        CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetStatsAsync(cancellationToken);

        return result.ToHttpResult(
            onSuccess: Results.Ok,
            statusCodeOnFailure: _ => StatusCodes.Status500InternalServerError);
    }
}
