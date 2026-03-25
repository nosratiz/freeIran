using Domain.Aggregates;
using FluentAssertions;
using Infrastructure.Persistence;

namespace UnitTests.Infrastructure;

/// <summary>
/// Unit tests for RegistrantEntity mapping to/from domain objects.
/// </summary>
public class RegistrantEntityTests
{
    #region Test Data Factory

    private static Registrant CreateDomainRegistrant() => new()
    {
        Id = "test-id-12345",
        Profile = new RegistrantProfile(
            Name: "John Doe",
            Email: "john.doe@example.com",
            DateOfBirth: new DateOnly(1990, 5, 15),
            Country: "US",
            Gender: Gender.Male,
            EducationLevel: EducationLevel.Masters),
        Skills = new SkillsProfile(
            LanguagesSpoken: ["English", "Persian", "French"],
            ProfessionalSkills: ["Software Development", "Project Management"]),
        Contribution = new BusinessContribution(
            CanRunBusiness: true,
            CanDonate: true),
        CreatedAtUtc = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
        UpdatedAtUtc = new DateTime(2024, 6, 20, 14, 45, 30, DateTimeKind.Utc)
    };

    private static RegistrantEntity CreateEntity() => new()
    {
        Id = "test-id-67890",
        Name = "Jane Smith",
        Email = "jane.smith@example.com",
        DateOfBirth = "1985-08-22",
        Country = "IR",
        Gender = "Female",
        EducationLevel = "Doctorate",
        LanguagesSpoken = ["Persian", "English"],
        ProfessionalSkills = ["Research", "Teaching"],
        CanRunBusiness = false,
        CanDonate = true,
        CreatedAtUtc = "2024-02-10T08:00:00.0000000Z",
        UpdatedAtUtc = null
    };

    #endregion

    #region FromDomain Tests

    [Fact]
    public void FromDomain_ShouldMapIdCorrectly()
    {
        // Arrange
        var registrant = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.Id.Should().Be(registrant.Id);
    }

    [Fact]
    public void FromDomain_ShouldMapProfileFieldsCorrectly()
    {
        // Arrange
        var registrant = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.Name.Should().Be(registrant.Profile.Name);
        entity.Email.Should().Be(registrant.Profile.Email);
        entity.Country.Should().Be(registrant.Profile.Country);
        entity.Gender.Should().Be(registrant.Profile.Gender.ToString());
        entity.EducationLevel.Should().Be(registrant.Profile.EducationLevel.ToString());
    }

    [Fact]
    public void FromDomain_ShouldFormatDateOfBirthCorrectly()
    {
        // Arrange
        var registrant = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.DateOfBirth.Should().Be("1990-05-15");
    }

    [Fact]
    public void FromDomain_ShouldMapSkillsCorrectly()
    {
        // Arrange
        var registrant = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.LanguagesSpoken.Should().BeEquivalentTo(registrant.Skills.LanguagesSpoken);
        entity.ProfessionalSkills.Should().BeEquivalentTo(registrant.Skills.ProfessionalSkills);
    }

    [Fact]
    public void FromDomain_ShouldMapContributionCorrectly()
    {
        // Arrange
        var registrant = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.CanRunBusiness.Should().Be(registrant.Contribution.CanRunBusiness);
        entity.CanDonate.Should().Be(registrant.Contribution.CanDonate);
    }

    [Fact]
    public void FromDomain_ShouldFormatCreatedAtUtcCorrectly()
    {
        // Arrange
        var registrant = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.CreatedAtUtc.Should().Contain("2024-01-15");
        entity.CreatedAtUtc.Should().EndWith("Z");
    }

    [Fact]
    public void FromDomain_ShouldFormatUpdatedAtUtcWhenPresent()
    {
        // Arrange
        var registrant = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.UpdatedAtUtc.Should().NotBeNullOrEmpty();
        entity.UpdatedAtUtc.Should().Contain("2024-06-20");
    }

    [Fact]
    public void FromDomain_ShouldSetUpdatedAtUtcToNullWhenNotPresent()
    {
        // Arrange
        var registrant = CreateDomainRegistrant() with { UpdatedAtUtc = null };

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void FromDomain_WithEmptySkills_ShouldMapToEmptyLists()
    {
        // Arrange
        var registrant = CreateDomainRegistrant() with
        {
            Skills = new SkillsProfile([], [])
        };

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.LanguagesSpoken.Should().BeEmpty();
        entity.ProfessionalSkills.Should().BeEmpty();
    }

    [Theory]
    [InlineData(Gender.Male)]
    [InlineData(Gender.Female)]
    [InlineData(Gender.NonBinary)]
    [InlineData(Gender.PreferNotToSay)]
    public void FromDomain_ShouldMapAllGenderValues(Gender gender)
    {
        // Arrange
        var registrant = CreateDomainRegistrant() with
        {
            Profile = CreateDomainRegistrant().Profile with { Gender = gender }
        };

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.Gender.Should().Be(gender.ToString());
    }

    [Theory]
    [InlineData(EducationLevel.NoFormalEducation)]
    [InlineData(EducationLevel.HighSchool)]
    [InlineData(EducationLevel.Bachelors)]
    [InlineData(EducationLevel.Masters)]
    [InlineData(EducationLevel.Doctorate)]
    public void FromDomain_ShouldMapAllEducationLevelValues(EducationLevel education)
    {
        // Arrange
        var registrant = CreateDomainRegistrant() with
        {
            Profile = CreateDomainRegistrant().Profile with { EducationLevel = education }
        };

        // Act
        var entity = RegistrantEntity.FromDomain(registrant);

        // Assert
        entity.EducationLevel.Should().Be(education.ToString());
    }

    #endregion

    #region ToDomain Tests

    [Fact]
    public void ToDomain_ShouldMapIdCorrectly()
    {
        // Arrange
        var entity = CreateEntity();

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.Id.Should().Be(entity.Id);
    }

    [Fact]
    public void ToDomain_ShouldMapProfileFieldsCorrectly()
    {
        // Arrange
        var entity = CreateEntity();

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.Profile.Name.Should().Be(entity.Name);
        registrant.Profile.Email.Should().Be(entity.Email);
        registrant.Profile.Country.Should().Be(entity.Country);
        registrant.Profile.Gender.Should().Be(Gender.Female);
        registrant.Profile.EducationLevel.Should().Be(EducationLevel.Doctorate);
    }

    [Fact]
    public void ToDomain_ShouldParseDateOfBirthCorrectly()
    {
        // Arrange
        var entity = CreateEntity();

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.Profile.DateOfBirth.Should().Be(new DateOnly(1985, 8, 22));
    }

    [Fact]
    public void ToDomain_ShouldMapSkillsCorrectly()
    {
        // Arrange
        var entity = CreateEntity();

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.Skills.LanguagesSpoken.Should().BeEquivalentTo(entity.LanguagesSpoken);
        registrant.Skills.ProfessionalSkills.Should().BeEquivalentTo(entity.ProfessionalSkills);
    }

    [Fact]
    public void ToDomain_ShouldMapContributionCorrectly()
    {
        // Arrange
        var entity = CreateEntity();

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.Contribution.CanRunBusiness.Should().Be(entity.CanRunBusiness);
        registrant.Contribution.CanDonate.Should().Be(entity.CanDonate);
    }

    [Fact]
    public void ToDomain_ShouldParseCreatedAtUtcCorrectly()
    {
        // Arrange
        var entity = CreateEntity();

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.CreatedAtUtc.Should().Be(new DateTime(2024, 2, 10, 8, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ToDomain_WhenUpdatedAtUtcIsNull_ShouldReturnNull()
    {
        // Arrange
        var entity = CreateEntity();
        entity.UpdatedAtUtc = null;

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ToDomain_WhenUpdatedAtUtcIsEmpty_ShouldReturnNull()
    {
        // Arrange
        var entity = CreateEntity();
        entity.UpdatedAtUtc = "";

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ToDomain_WhenUpdatedAtUtcHasValue_ShouldParseCorrectly()
    {
        // Arrange
        var entity = CreateEntity();
        entity.UpdatedAtUtc = "2024-03-15T12:30:45.0000000Z";

        // Act
        var registrant = entity.ToDomain();

        // Assert
        registrant.UpdatedAtUtc.Should().Be(new DateTime(2024, 3, 15, 12, 30, 45, DateTimeKind.Utc));
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_FromDomainToDomain_ShouldPreserveAllValues()
    {
        // Arrange
        var original = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(original);
        var restored = entity.ToDomain();

        // Assert
        restored.Id.Should().Be(original.Id);
        restored.Profile.Name.Should().Be(original.Profile.Name);
        restored.Profile.Email.Should().Be(original.Profile.Email);
        restored.Profile.DateOfBirth.Should().Be(original.Profile.DateOfBirth);
        restored.Profile.Country.Should().Be(original.Profile.Country);
        restored.Profile.Gender.Should().Be(original.Profile.Gender);
        restored.Profile.EducationLevel.Should().Be(original.Profile.EducationLevel);
        restored.Skills.LanguagesSpoken.Should().BeEquivalentTo(original.Skills.LanguagesSpoken);
        restored.Skills.ProfessionalSkills.Should().BeEquivalentTo(original.Skills.ProfessionalSkills);
        restored.Contribution.CanRunBusiness.Should().Be(original.Contribution.CanRunBusiness);
        restored.Contribution.CanDonate.Should().Be(original.Contribution.CanDonate);
    }

    [Fact]
    public void RoundTrip_ShouldPreserveDateTimeKind()
    {
        // Arrange
        var original = CreateDomainRegistrant();

        // Act
        var entity = RegistrantEntity.FromDomain(original);
        var restored = entity.ToDomain();

        // Assert
        restored.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
        if (restored.UpdatedAtUtc.HasValue)
        {
            restored.UpdatedAtUtc.Value.Kind.Should().Be(DateTimeKind.Utc);
        }
    }

    [Theory]
    [InlineData("2024-01-01")]
    [InlineData("1990-12-31")]
    [InlineData("2000-06-15")]
    public void RoundTrip_ShouldPreserveDateOfBirth(string dateString)
    {
        // Arrange
        var date = DateOnly.Parse(dateString);
        var original = CreateDomainRegistrant() with
        {
            Profile = CreateDomainRegistrant().Profile with { DateOfBirth = date }
        };

        // Act
        var entity = RegistrantEntity.FromDomain(original);
        var restored = entity.ToDomain();

        // Assert
        restored.Profile.DateOfBirth.Should().Be(date);
    }

    #endregion
}
