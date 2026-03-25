using Domain.Aggregates;
using Domain.Constants;
using Domain.Enums;
using Domain.ValueObjects;
using FluentAssertions;

namespace UnitTests.Domain;

/// <summary>
/// Unit tests for the Registrant aggregate root.
/// </summary>
public class RegistrantTests
{
    #region Test Data Factory

    private static Registrant CreateRegistrant(DateOnly dateOfBirth) => new()
    {
        Id = "test-id-123",
        Profile = new RegistrantProfile(
            Name: "Test User",
            Email: "test@example.com",
            DateOfBirth: dateOfBirth,
            Country: "US",
            Gender: Gender.Male,
            EducationLevel: EducationLevel.Bachelors),
        Skills = new SkillsProfile(
            LanguagesSpoken: ["English", "Persian"],
            ProfessionalSkills: ["Software Development"]),
        Contribution = new BusinessContribution(
            CanRunBusiness: true,
            CanDonate: true),
        CreatedAtUtc = DateTime.UtcNow
    };

    private static DateOnly GetDateOfBirthForAge(int age)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return today.AddYears(-age).AddDays(-1); // Add -1 day to ensure the birthday has passed
    }

    #endregion

    #region GetAgeGroup Tests

    [Fact]
    public void GetAgeGroup_WhenAgeIs10_ReturnsUnder18()
    {
        // Arrange
        var registrant = CreateRegistrant(GetDateOfBirthForAge(10));

        // Act
        var result = registrant.GetAgeGroup();

        // Assert
        result.Should().Be(AgeGroup.Under18);
    }

    [Theory]
    [InlineData(17)]
    [InlineData(15)]
    [InlineData(10)]
    [InlineData(5)]
    public void GetAgeGroup_WhenAgeIsAtOrBelowUnder18Max_ReturnsUnder18(int age)
    {
        // Arrange
        var registrant = CreateRegistrant(GetDateOfBirthForAge(age));

        // Act
        var result = registrant.GetAgeGroup();

        // Assert
        result.Should().Be(AgeGroup.Under18);
        age.Should().BeLessThanOrEqualTo(AgeGroupBoundaries.Under18Max);
    }

    [Theory]
    [InlineData(18)]
    [InlineData(20)]
    [InlineData(24)]
    public void GetAgeGroup_WhenAgeBetween18And24_ReturnsAge18To24(int age)
    {
        // Arrange
        var registrant = CreateRegistrant(GetDateOfBirthForAge(age));

        // Act
        var result = registrant.GetAgeGroup();

        // Assert
        result.Should().Be(AgeGroup.Age18To24);
    }

    [Theory]
    [InlineData(25)]
    [InlineData(30)]
    [InlineData(34)]
    public void GetAgeGroup_WhenAgeBetween25And34_ReturnsAge25To34(int age)
    {
        // Arrange
        var registrant = CreateRegistrant(GetDateOfBirthForAge(age));

        // Act
        var result = registrant.GetAgeGroup();

        // Assert
        result.Should().Be(AgeGroup.Age25To34);
    }

    [Theory]
    [InlineData(35)]
    [InlineData(40)]
    [InlineData(44)]
    public void GetAgeGroup_WhenAgeBetween35And44_ReturnsAge35To44(int age)
    {
        // Arrange
        var registrant = CreateRegistrant(GetDateOfBirthForAge(age));

        // Act
        var result = registrant.GetAgeGroup();

        // Assert
        result.Should().Be(AgeGroup.Age35To44);
    }

    [Theory]
    [InlineData(45)]
    [InlineData(50)]
    [InlineData(54)]
    public void GetAgeGroup_WhenAgeBetween45And54_ReturnsAge45To54(int age)
    {
        // Arrange
        var registrant = CreateRegistrant(GetDateOfBirthForAge(age));

        // Act
        var result = registrant.GetAgeGroup();

        // Assert
        result.Should().Be(AgeGroup.Age45To54);
    }

    [Theory]
    [InlineData(55)]
    [InlineData(60)]
    [InlineData(64)]
    public void GetAgeGroup_WhenAgeBetween55And64_ReturnsAge55To64(int age)
    {
        // Arrange
        var registrant = CreateRegistrant(GetDateOfBirthForAge(age));

        // Act
        var result = registrant.GetAgeGroup();

        // Assert
        result.Should().Be(AgeGroup.Age55To64);
    }

    [Theory]
    [InlineData(65)]
    [InlineData(70)]
    [InlineData(80)]
    [InlineData(100)]
    public void GetAgeGroup_WhenAge65OrOlder_ReturnsAge65Plus(int age)
    {
        // Arrange
        var registrant = CreateRegistrant(GetDateOfBirthForAge(age));

        // Act
        var result = registrant.GetAgeGroup();

        // Assert
        result.Should().Be(AgeGroup.Age65Plus);
    }

    [Fact]
    public void GetAgeGroup_AtBoundary17To18_ReturnsCorrectGroup()
    {
        // Arrange - exactly 17 years old
        var registrant17 = CreateRegistrant(GetDateOfBirthForAge(17));
        var registrant18 = CreateRegistrant(GetDateOfBirthForAge(18));

        // Act & Assert
        registrant17.GetAgeGroup().Should().Be(AgeGroup.Under18);
        registrant18.GetAgeGroup().Should().Be(AgeGroup.Age18To24);
    }

    [Fact]
    public void GetAgeGroup_AtBoundary64To65_ReturnsCorrectGroup()
    {
        // Arrange
        var registrant64 = CreateRegistrant(GetDateOfBirthForAge(64));
        var registrant65 = CreateRegistrant(GetDateOfBirthForAge(65));

        // Act & Assert
        registrant64.GetAgeGroup().Should().Be(AgeGroup.Age55To64);
        registrant65.GetAgeGroup().Should().Be(AgeGroup.Age65Plus);
    }

    #endregion

    #region Record Equality Tests

    [Fact]
    public void Registrant_WithSameValues_ShouldBeEquivalent()
    {
        // Arrange
        var dob = new DateOnly(1990, 5, 15);
        var createdAt = DateTime.UtcNow;
        
        var registrant1 = new Registrant
        {
            Id = "same-id",
            Profile = new RegistrantProfile("Test", "test@test.com", dob, "US", Gender.Male, EducationLevel.Bachelors),
            Skills = new SkillsProfile(["English"], ["Dev"]),
            Contribution = new BusinessContribution(true, true),
            CreatedAtUtc = createdAt
        };

        var registrant2 = new Registrant
        {
            Id = "same-id",
            Profile = new RegistrantProfile("Test", "test@test.com", dob, "US", Gender.Male, EducationLevel.Bachelors),
            Skills = new SkillsProfile(["English"], ["Dev"]),
            Contribution = new BusinessContribution(true, true),
            CreatedAtUtc = createdAt
        };

        // Act & Assert - Using structural equivalence for records with collection properties
        registrant1.Should().BeEquivalentTo(registrant2);
    }

    [Fact]
    public void Registrant_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var dob = new DateOnly(1990, 5, 15);
        var registrant1 = CreateRegistrant(dob) with { Id = "id-1" };
        var registrant2 = CreateRegistrant(dob) with { Id = "id-2" };

        // Act & Assert
        registrant1.Should().NotBe(registrant2);
    }

    #endregion

    #region Value Object Tests

    [Fact]
    public void RegistrantProfile_ShouldStoreAllProperties()
    {
        // Arrange & Act
        var profile = new RegistrantProfile(
            Name: "John Doe",
            Email: "john@example.com",
            DateOfBirth: new DateOnly(1985, 3, 20),
            Country: "IR",
            Gender: Gender.Male,
            EducationLevel: EducationLevel.Masters);

        // Assert
        profile.Name.Should().Be("John Doe");
        profile.Email.Should().Be("john@example.com");
        profile.DateOfBirth.Should().Be(new DateOnly(1985, 3, 20));
        profile.Country.Should().Be("IR");
        profile.Gender.Should().Be(Gender.Male);
        profile.EducationLevel.Should().Be(EducationLevel.Masters);
    }

    [Fact]
    public void SkillsProfile_ShouldStoreAllProperties()
    {
        // Arrange & Act
        var skills = new SkillsProfile(
            LanguagesSpoken: ["Persian", "English", "Arabic"],
            ProfessionalSkills: ["Engineering", "Project Management"]);

        // Assert
        skills.LanguagesSpoken.Should().HaveCount(3);
        skills.LanguagesSpoken.Should().Contain("Persian");
        skills.ProfessionalSkills.Should().HaveCount(2);
        skills.ProfessionalSkills.Should().Contain("Engineering");
    }

    [Fact]
    public void BusinessContribution_ShouldStoreAllProperties()
    {
        // Arrange & Act
        var contribution = new BusinessContribution(
            CanRunBusiness: true,
            CanDonate: false);

        // Assert
        contribution.CanRunBusiness.Should().BeTrue();
        contribution.CanDonate.Should().BeFalse();
    }

    #endregion

    #region Enum Tests

    [Fact]
    public void Gender_ShouldHaveAllExpectedValues()
    {
        // Assert
        Enum.GetValues<Gender>().Should().HaveCount(4);
        Enum.IsDefined(Gender.Male).Should().BeTrue();
        Enum.IsDefined(Gender.Female).Should().BeTrue();
        Enum.IsDefined(Gender.NonBinary).Should().BeTrue();
        Enum.IsDefined(Gender.PreferNotToSay).Should().BeTrue();
    }

    [Fact]
    public void EducationLevel_ShouldHaveAllExpectedValues()
    {
        // Assert
        Enum.GetValues<EducationLevel>().Should().HaveCount(8);
        Enum.IsDefined(EducationLevel.NoFormalEducation).Should().BeTrue();
        Enum.IsDefined(EducationLevel.Doctorate).Should().BeTrue();
    }

    [Fact]
    public void AgeGroup_ShouldHaveAllExpectedValues()
    {
        // Assert
        Enum.GetValues<AgeGroup>().Should().HaveCount(7);
        Enum.IsDefined(AgeGroup.Under18).Should().BeTrue();
        Enum.IsDefined(AgeGroup.Age65Plus).Should().BeTrue();
    }

    #endregion
}
