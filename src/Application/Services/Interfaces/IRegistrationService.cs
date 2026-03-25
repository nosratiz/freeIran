using Application.DTOs;
using Domain.Aggregates;
using FluentResults;

namespace Application.Services.Interfaces;

/// <summary>
/// Registration service interface for dependency injection.
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Registers a new participant in the portal.
    /// </summary>
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a registrant by their ID.
    /// </summary>
    Task<Result<Registrant>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
