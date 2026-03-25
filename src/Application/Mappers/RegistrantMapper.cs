using Application.DTOs;
using Domain.Aggregates;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Mappers;

/// <summary>
/// Mapper for converting between RegisterRequest DTOs and domain Registrant entities.
/// </summary>
public static class RegistrantMapper
{
    /// <summary>
    /// Maps a registration request to a domain Registrant entity.
    /// </summary>
    public static Registrant ToRegistrant(this RegisterRequest request)
    {
        return new Registrant
        {
            Id = Ulid.NewUlid().ToString(),
            Profile = new RegistrantProfile(
                request.Name.Trim(),
                request.Email.Trim().ToLowerInvariant(),
                request.DateOfBirth,
                request.Country.Trim().ToUpperInvariant(),
                request.Gender,
                request.EducationLevel),
            Skills = new SkillsProfile(
                request.LanguagesSpoken
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(l => l.Trim())
                    .Distinct()
                    .ToList(),
                request.ProfessionalSkills
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Distinct()
                    .ToList()),
            Contribution = new BusinessContribution(
                request.CanRunBusiness,
                request.CanDonate),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a domain Registrant to a RegisterResponse DTO.
    /// </summary>
    public static RegisterResponse ToResponse(this Registrant registrant)
    {
        return new RegisterResponse(
            registrant.Id,
            registrant.Profile.Name,
            registrant.Profile.Email,
            registrant.CreatedAtUtc);
    }
}
