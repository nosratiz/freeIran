namespace Domain.ValueObjects;

/// <summary>
/// Value object representing the registrant's business contribution preferences.
/// </summary>
public sealed record BusinessContribution(
    bool CanRunBusiness,
    bool CanDonate);
