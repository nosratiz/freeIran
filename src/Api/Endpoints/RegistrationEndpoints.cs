using Api.Constants;
using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Errors;
using FluentResults;

namespace Api.Endpoints;

/// <summary>
/// Minimal API endpoint definitions for the registration portal.
/// </summary>
public static class RegistrationEndpoints
{
    /// <summary>
    /// Maps all registration-related endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapRegistrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiConstants.Routes.ApiPrefix)
            .WithTags(ApiConstants.Tags.Registration)
            .WithOpenApi();

        // POST /api/register
        group.MapPost(ApiConstants.Routes.Register, RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new participant")
            .WithDescription("Registers a new participant in the Free Iran Reconstruction Portal.")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError)
            .RequireRateLimiting(ApiConstants.Policies.FixedRateLimiter);

        // GET /api/registrants/{id}
        group.MapGet(ApiConstants.Routes.RegistrantsById, GetRegistrantByIdAsync)
            .WithName("GetRegistrant")
            .WithSummary("Get a registrant by ID")
            .WithDescription("Retrieves a registrant's details by their unique identifier.")
            .Produces<RegisterResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireRateLimiting(ApiConstants.Policies.FixedRateLimiter);

        return app;
    }

    /// <summary>
    /// Handles POST /api/register
    /// </summary>
    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IRegistrationService registrationService,
        CancellationToken cancellationToken)
    {
        var result = await registrationService.RegisterAsync(request, cancellationToken);

        return result.ToHttpResult(
            onSuccess: response => Results.Created($"{ApiConstants.Routes.ApiPrefix}/registrants/{response.Id}", response),
            statusCodeOnFailure: error => error.HasMetadataKey("Code") && 
                                          error.Metadata["Code"]?.ToString() == DomainErrors.Registrant.EmailAlreadyExists
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Handles GET /api/registrants/{id}
    /// </summary>
    private static async Task<IResult> GetRegistrantByIdAsync(
        string id,
        IRegistrationService registrationService,
        CancellationToken cancellationToken)
    {
        var result = await registrationService.GetByIdAsync(id, cancellationToken);

        return result.ToHttpResult(
            onSuccess: registrant => Results.Ok(new RegisterResponse(
                registrant.Id,
                registrant.Profile.Name,
                registrant.Profile.Email,
                registrant.CreatedAtUtc)),
            statusCodeOnFailure: error => error.HasMetadataKey("Code") && 
                                          error.Metadata["Code"]?.ToString() == DomainErrors.Registrant.NotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest);
    }
}
