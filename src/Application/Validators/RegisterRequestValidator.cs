using Application.Constants;
using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

/// <summary>
/// FluentValidation validator for registration requests.
/// Uses centralized validation constants and error messages.
/// </summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // Name validation
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ErrorMessages.Name.Required)
            .MinimumLength(ValidationConstants.Name.MinLength)
            .WithMessage(ErrorMessages.Name.GetMinLengthMessage(ValidationConstants.Name.MinLength))
            .MaximumLength(ValidationConstants.Name.MaxLength)
            .WithMessage(ErrorMessages.Name.GetMaxLengthMessage(ValidationConstants.Name.MaxLength))
            .Matches(ValidationConstants.Name.Pattern)
            .WithMessage(ErrorMessages.Name.InvalidFormat);

        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(ErrorMessages.Email.Required)
            .MaximumLength(ValidationConstants.Email.MaxLength)
            .WithMessage(ErrorMessages.Email.GetMaxLengthMessage(ValidationConstants.Email.MaxLength))
            .EmailAddress()
            .WithMessage(ErrorMessages.Email.InvalidFormat);

        // Date of Birth validation
        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage(ErrorMessages.DateOfBirth.Required)
            .Must(BeAValidAge)
            .WithMessage(ErrorMessages.DateOfBirth.GetMinimumAgeMessage(ValidationConstants.Age.MinimumAge))
            .Must(BeNotTooOld)
            .WithMessage(ErrorMessages.DateOfBirth.MaximumAge);

        // Country validation
        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage(ErrorMessages.Country.Required)
            .Must(CountryCodes.IsValid)
            .WithMessage(ErrorMessages.Country.InvalidCode);

        // Gender validation
        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage(ErrorMessages.Gender.Invalid);

        // Education Level validation
        RuleFor(x => x.EducationLevel)
            .IsInEnum()
            .WithMessage(ErrorMessages.EducationLevel.Invalid);

        // Languages validation
        RuleFor(x => x.LanguagesSpoken)
            .NotNull()
            .WithMessage(ErrorMessages.Languages.NullList)
            .Must(l => l.Count >= ValidationConstants.Languages.MinCount)
            .WithMessage(ErrorMessages.Languages.MinCount)
            .Must(l => l.Count <= ValidationConstants.Languages.MaxCount)
            .WithMessage(ErrorMessages.Languages.GetMaxCountMessage(ValidationConstants.Languages.MaxCount))
            .ForEach(language =>
            {
                language
                    .NotEmpty()
                    .WithMessage(ErrorMessages.Languages.EmptyName)
                    .MaximumLength(ValidationConstants.Languages.MaxNameLength)
                    .WithMessage(ErrorMessages.Languages.GetMaxNameLengthMessage(ValidationConstants.Languages.MaxNameLength));
            });

        // Professional Skills validation
        RuleFor(x => x.ProfessionalSkills)
            .NotNull()
            .WithMessage(ErrorMessages.Skills.NullList)
            .Must(s => s.Count <= ValidationConstants.Skills.MaxCount)
            .WithMessage(ErrorMessages.Skills.GetMaxCountMessage(ValidationConstants.Skills.MaxCount))
            .ForEach(skill =>
            {
                skill
                    .MaximumLength(ValidationConstants.Skills.MaxDescriptionLength)
                    .WithMessage(ErrorMessages.Skills.GetMaxDescriptionLengthMessage(ValidationConstants.Skills.MaxDescriptionLength));
            });
    }

    private static bool BeAValidAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;
        
        if (dateOfBirth > today.AddYears(-age))
            age--;

        return age >= ValidationConstants.Age.MinimumAge;
    }

    private static bool BeNotTooOld(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;
        
        if (dateOfBirth > today.AddYears(-age))
            age--;

        return age <= ValidationConstants.Age.MaximumAge;
    }
}
