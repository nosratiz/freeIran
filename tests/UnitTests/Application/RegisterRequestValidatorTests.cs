using Application.Constants;
using Application.DTOs;
using Application.Validators;
using Domain.Aggregates;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace UnitTests.Application;

/// <summary>
/// Unit tests for RegisterRequestValidator.
/// </summary>
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    #region Test Data Factory

    private static RegisterRequest CreateValidRequest() => new()
    {
        Name = "John Doe",
        Email = "john.doe@example.com",
        DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
        Country = "US",
        Gender = Gender.Male,
        EducationLevel = EducationLevel.Bachelors,
        LanguagesSpoken = ["English", "Persian"],
        ProfessionalSkills = ["Software Development"],
        CanRunBusiness = true,
        CanDonate = true
    };

    private static DateOnly GetDateOfBirthForAge(int age)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return today.AddYears(-age).AddDays(-1);
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { Name = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(ErrorMessages.Name.Required);
    }

    [Fact]
    public void Validate_WhenNameIsTooShort_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { Name = "A" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhenNameIsTooLong_ShouldHaveError()
    {
        // Arrange
        var longName = new string('A', ValidationConstants.Name.MaxLength + 1);
        var request = CreateValidRequest() with { Name = longName };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("John@Doe")]
    [InlineData("John#Doe")]
    public void Validate_WhenNameHasInvalidCharacters_ShouldHaveError(string invalidName)
    {
        // Arrange
        var request = CreateValidRequest() with { Name = invalidName };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(ErrorMessages.Name.InvalidFormat);
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("Mary-Jane Watson")]
    [InlineData("O'Brien")]
    [InlineData("مریم")]  // Persian name
    [InlineData("李明")]   // Chinese name
    public void Validate_WhenNameIsValid_ShouldNotHaveError(string validName)
    {
        // Arrange
        var request = CreateValidRequest() with { Name = validName };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { Email = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(ErrorMessages.Email.Required);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("plainaddress")]
    public void Validate_WhenEmailIsInvalid_ShouldHaveError(string invalidEmail)
    {
        // Arrange
        var request = CreateValidRequest() with { Email = invalidEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+label@gmail.com")]
    public void Validate_WhenEmailIsValid_ShouldNotHaveError(string validEmail)
    {
        // Arrange
        var request = CreateValidRequest() with { Email = validEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@b.co"; // Total > 254
        var request = CreateValidRequest() with { Email = longEmail };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region DateOfBirth Validation Tests

    [Fact]
    public void Validate_WhenAgeIsBelowMinimum_ShouldHaveError()
    {
        // Arrange - age 10, below minimum 13
        var request = CreateValidRequest() with 
        { 
            DateOfBirth = GetDateOfBirthForAge(10) 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_WhenAgeExceedsMaximum_ShouldHaveError()
    {
        // Arrange - age 130, above maximum 120
        var request = CreateValidRequest() with 
        { 
            DateOfBirth = GetDateOfBirthForAge(130) 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Theory]
    [InlineData(13)]  // Minimum age
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(120)] // Maximum age
    public void Validate_WhenAgeIsValid_ShouldNotHaveError(int age)
    {
        // Arrange
        var request = CreateValidRequest() with 
        { 
            DateOfBirth = GetDateOfBirthForAge(age) 
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    #endregion

    #region Country Validation Tests

    [Fact]
    public void Validate_WhenCountryIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { Country = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country)
            .WithErrorMessage(ErrorMessages.Country.Required);
    }

    [Fact]
    public void Validate_WhenCountryIsInvalid_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { Country = "INVALID" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country)
            .WithErrorMessage(ErrorMessages.Country.InvalidCode);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("IR")]
    [InlineData("GB")]
    [InlineData("DE")]
    public void Validate_WhenCountryIsValid_ShouldNotHaveError(string countryCode)
    {
        // Arrange
        var request = CreateValidRequest() with { Country = countryCode };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Country);
    }

    #endregion

    #region Languages Validation Tests

    [Fact]
    public void Validate_WhenLanguagesIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { LanguagesSpoken = [] };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LanguagesSpoken)
            .WithErrorMessage(ErrorMessages.Languages.MinCount);
    }

    [Fact]
    public void Validate_WhenLanguagesExceedsMax_ShouldHaveError()
    {
        // Arrange
        var tooManyLanguages = Enumerable.Range(1, ValidationConstants.Languages.MaxCount + 1)
            .Select(i => $"Language{i}")
            .ToList();
        var request = CreateValidRequest() with { LanguagesSpoken = tooManyLanguages };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LanguagesSpoken);
    }

    [Fact]
    public void Validate_WhenLanguageNameTooLong_ShouldHaveError()
    {
        // Arrange
        var longLanguageName = new string('A', ValidationConstants.Languages.MaxNameLength + 1);
        var request = CreateValidRequest() with { LanguagesSpoken = [longLanguageName] };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validate_WhenLanguageNameIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { LanguagesSpoken = ["English", ""] };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(20)]
    public void Validate_WhenLanguageCountIsValid_ShouldNotHaveError(int count)
    {
        // Arrange
        var languages = Enumerable.Range(1, count)
            .Select(i => $"Language{i}")
            .ToList();
        var request = CreateValidRequest() with { LanguagesSpoken = languages };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LanguagesSpoken);
    }

    #endregion

    #region Skills Validation Tests

    [Fact]
    public void Validate_WhenSkillsExceedsMax_ShouldHaveError()
    {
        // Arrange
        var tooManySkills = Enumerable.Range(1, ValidationConstants.Skills.MaxCount + 1)
            .Select(i => $"Skill{i}")
            .ToList();
        var request = CreateValidRequest() with { ProfessionalSkills = tooManySkills };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProfessionalSkills);
    }

    [Fact]
    public void Validate_WhenSkillDescriptionTooLong_ShouldHaveError()
    {
        // Arrange
        var longSkill = new string('A', ValidationConstants.Skills.MaxDescriptionLength + 1);
        var request = CreateValidRequest() with { ProfessionalSkills = [longSkill] };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public void Validate_WhenSkillsIsEmpty_ShouldNotHaveError()
    {
        // Arrange - Skills can be empty
        var request = CreateValidRequest() with { ProfessionalSkills = [] };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProfessionalSkills);
    }

    #endregion

    #region Enum Validation Tests

    [Fact]
    public void Validate_WhenGenderIsInvalidEnum_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { Gender = (Gender)999 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender)
            .WithErrorMessage(ErrorMessages.Gender.Invalid);
    }

    [Fact]
    public void Validate_WhenEducationLevelIsInvalidEnum_ShouldHaveError()
    {
        // Arrange
        var request = CreateValidRequest() with { EducationLevel = (EducationLevel)999 };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EducationLevel)
            .WithErrorMessage(ErrorMessages.EducationLevel.Invalid);
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.NonBinary)]
    [InlineData(Gender.PreferNotToSay)]
    public void Validate_WhenGenderIsValid_ShouldNotHaveError(Gender gender)
    {
        // Arrange
        var request = CreateValidRequest() with { Gender = gender };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    #endregion

    #region Complete Validation Tests

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenMultipleFieldsInvalid_ShouldHaveMultipleErrors()
    {
        // Arrange
        var request = CreateValidRequest() with 
        { 
            Name = "", 
            Email = "invalid",
            Country = "INVALID",
            LanguagesSpoken = []
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);
    }

    #endregion
}
