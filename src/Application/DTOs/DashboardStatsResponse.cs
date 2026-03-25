namespace Application.DTOs;

/// <summary>
/// Dashboard statistics response DTO.
/// </summary>
public sealed record DashboardStatsResponse
{
    public required int TotalRegistrations { get; init; }
    public required IReadOnlyDictionary<string, int> ByCountry { get; init; }
    public required IReadOnlyDictionary<string, int> ByGender { get; init; }
    public required IReadOnlyDictionary<string, int> ByAgeGroup { get; init; }
    public required IReadOnlyDictionary<string, int> ByEducationLevel { get; init; }
    public required BusinessStats BusinessContributions { get; init; }
    public required DateTime GeneratedAtUtc { get; init; }
}

/// <summary>
/// Business contribution statistics.
/// </summary>
public sealed record BusinessStats(
    int CanRunBusinessCount,
    int CanDonateCount,
    int BothCount);
