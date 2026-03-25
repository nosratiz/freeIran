using Domain.Aggregates;
using FluentResults;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Registrant aggregate persistence.
/// </summary>
public interface IRegistrantRepository
{
    /// <summary>
    /// Saves a new registrant to the data store.
    /// </summary>
    Task<Result<Registrant>> CreateAsync(Registrant registrant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a registrant by their unique identifier.
    /// </summary>
    Task<Result<Registrant>> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a registrant by their email address.
    /// </summary>
    Task<Result<Registrant?>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all registrants. Use with caution for large datasets.
    /// </summary>
    Task<Result<IReadOnlyList<Registrant>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a registrant with the given email already exists.
    /// </summary>
    Task<Result<bool>> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
