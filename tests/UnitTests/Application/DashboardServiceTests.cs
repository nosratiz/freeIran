using Application.Constants;
using Application.Services;
using Domain.Aggregates;
using Domain.Repositories;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.Application;

/// <summary>
/// Unit tests for DashboardService.
/// </summary>
public class DashboardServiceTests
{
    private readonly IRegistrantRepository _repositoryMock;
    private readonly ILogger<DashboardService> _loggerMock;
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _repositoryMock = Substitute.For<IRegistrantRepository>();
        _loggerMock = Substitute.For<ILogger<DashboardService>>();
        _sut = new DashboardService(_repositoryMock, _loggerMock);
    }

    #region Test Data Factory

    private static Registrant CreateRegistrant(
        string country = "US",
        Gender gender = Gender.Male,
        int age = 25,
        EducationLevel education = EducationLevel.Bachelors,
        bool canRunBusiness = false,
        bool canDonate = false)
    {
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-age).AddDays(-1));
        
        return new Registrant
        {
            Id = Ulid.NewUlid().ToString(),
            Profile = new RegistrantProfile(
                Name: "Test User",
                Email: "test@example.com",
                DateOfBirth: dob,
                Country: country,
                Gender: gender,
                EducationLevel: education),
            Skills = new SkillsProfile(["English"], ["Dev"]),
            Contribution = new BusinessContribution(canRunBusiness, canDonate),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    #endregion

    #region GetStatsAsync Tests

    [Fact]
    public async Task GetStatsAsync_WhenNoRegistrants_ReturnsEmptyStats()
    {
        // Arrange
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>([]));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRegistrations.Should().Be(0);
        result.Value.ByCountry.Should().BeEmpty();
        result.Value.ByGender.Should().BeEmpty();
        result.Value.ByAgeGroup.Should().BeEmpty();
        result.Value.ByEducationLevel.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatsAsync_WhenRepositoryFails_ReturnsFailure()
    {
        // Arrange
        var error = new Error("Database connection failed");
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Fail<IReadOnlyList<Registrant>>(error));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Database connection failed");
    }

    [Fact]
    public async Task GetStatsAsync_ShouldCalculateTotalRegistrations()
    {
        // Arrange
        var registrants = new List<Registrant>
        {
            CreateRegistrant(),
            CreateRegistrant(),
            CreateRegistrant()
        };
        
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>(registrants));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRegistrations.Should().Be(3);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldGroupByCountry()
    {
        // Arrange
        var registrants = new List<Registrant>
        {
            CreateRegistrant(country: "US"),
            CreateRegistrant(country: "US"),
            CreateRegistrant(country: "IR"),
            CreateRegistrant(country: "GB")
        };
        
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>(registrants));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ByCountry.Should().HaveCount(3);
        result.Value.ByCountry["US"].Should().Be(2);
        result.Value.ByCountry["IR"].Should().Be(1);
        result.Value.ByCountry["GB"].Should().Be(1);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldGroupByGender()
    {
        // Arrange
        var registrants = new List<Registrant>
        {
            CreateRegistrant(gender: Gender.Male),
            CreateRegistrant(gender: Gender.Male),
            CreateRegistrant(gender: Gender.Female),
            CreateRegistrant(gender: Gender.NonBinary)
        };
        
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>(registrants));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ByGender.Should().HaveCount(3);
        result.Value.ByGender["Male"].Should().Be(2);
        result.Value.ByGender["Female"].Should().Be(1);
        result.Value.ByGender["NonBinary"].Should().Be(1);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldGroupByAgeGroup()
    {
        // Arrange
        var registrants = new List<Registrant>
        {
            CreateRegistrant(age: 15),  // Under18
            CreateRegistrant(age: 20),  // Age18To24
            CreateRegistrant(age: 30),  // Age25To34
            CreateRegistrant(age: 30),  // Age25To34
            CreateRegistrant(age: 50),  // Age45To54
            CreateRegistrant(age: 70)   // Age65Plus
        };
        
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>(registrants));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ByAgeGroup.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldGroupByEducationLevel()
    {
        // Arrange
        var registrants = new List<Registrant>
        {
            CreateRegistrant(education: EducationLevel.Bachelors),
            CreateRegistrant(education: EducationLevel.Bachelors),
            CreateRegistrant(education: EducationLevel.Masters),
            CreateRegistrant(education: EducationLevel.Doctorate)
        };
        
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>(registrants));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ByEducationLevel.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldCalculateBusinessContributions()
    {
        // Arrange
        var registrants = new List<Registrant>
        {
            CreateRegistrant(canRunBusiness: true, canDonate: false),
            CreateRegistrant(canRunBusiness: false, canDonate: true),
            CreateRegistrant(canRunBusiness: true, canDonate: true),
            CreateRegistrant(canRunBusiness: true, canDonate: true),
            CreateRegistrant(canRunBusiness: false, canDonate: false)
        };
        
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>(registrants));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.BusinessContributions.CanRunBusinessCount.Should().Be(3);  // 1 + 2 with both
        result.Value.BusinessContributions.CanDonateCount.Should().Be(3);       // 1 + 2 with both
        result.Value.BusinessContributions.BothCount.Should().Be(2);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldSetGeneratedAtUtc()
    {
        // Arrange
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>([]));
        
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = await _sut.GetStatsAsync();
        
        var afterCall = DateTime.UtcNow;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.GeneratedAtUtc.Should().BeOnOrAfter(beforeCall);
        result.Value.GeneratedAtUtc.Should().BeOnOrBefore(afterCall);
    }

    [Fact]
    public async Task GetStatsAsync_ShouldSupportCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return await Task.FromResult(Result.Ok<IReadOnlyList<Registrant>>([]));
            });

        // Act
        var act = async () => await _sut.GetStatsAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetStatsAsync_WithLargeDataset_ShouldCompleteSuccessfully()
    {
        // Arrange
        var registrants = Enumerable.Range(1, 1000)
            .Select(i => CreateRegistrant(
                country: i % 10 == 0 ? "IR" : "US",
                gender: (Gender)(i % 4),
                age: 18 + (i % 50),
                education: (EducationLevel)(i % 8),
                canRunBusiness: i % 3 == 0,
                canDonate: i % 2 == 0))
            .ToList();
        
        _repositoryMock
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Ok<IReadOnlyList<Registrant>>(registrants));

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalRegistrations.Should().Be(1000);
        result.Value.ByCountry.Should().NotBeEmpty();
        result.Value.ByGender.Should().NotBeEmpty();
        result.Value.ByAgeGroup.Should().NotBeEmpty();
        result.Value.ByEducationLevel.Should().NotBeEmpty();
    }

    #endregion
}
