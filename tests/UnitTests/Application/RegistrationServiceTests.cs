using Application.Constants;
using Application.DTOs;
using Application.Services;
using Application.Validators;
using Domain.Aggregates;
using Domain.Enums;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UnitTests.Application;

/// <summary>
/// Unit tests for RegistrationService.
/// </summary>
public class RegistrationServiceTests
{
    private readonly IRegistrantRepository _repositoryMock;
    private readonly IValidator<RegisterRequest> _validator;
    private readonly ILogger<RegistrationService> _loggerMock;
    private readonly RegistrationService _sut;

    public RegistrationServiceTests()
    {
        _repositoryMock = Substitute.For<IRegistrantRepository>();
        _validator = new RegisterRequestValidator();
        _loggerMock = Substitute.For<ILogger<RegistrationService>>();
        _sut = new RegistrationService(_repositoryMock, _validator, _loggerMock);
    }

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

    private static Registrant CreateRegistrant(RegisterRequest request) => new()
    {
        Id = "test-id-123",
        Profile = new RegistrantProfile(
            request.Name,
            request.Email,
            request.DateOfBirth,
            request.Country,
            request.Gender,
            request.EducationLevel),
        Skills = new SkillsProfile(
            request.LanguagesSpoken.ToList(),
            request.ProfessionalSkills.ToList()),
        Contribution = new BusinessContribution(
            request.CanRunBusiness,
            request.CanDonate),
        CreatedAtUtc = DateTime.UtcNow
    };

    #endregion

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = CreateValidRequest();
        
        _repositoryMock
            .ExistsByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(false));
        
        _repositoryMock
            .CreateAsync(Arg.Any<Registrant>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.Ok(callInfo.Arg<Registrant>()));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(request.Email.ToLowerInvariant());
        result.Value.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task RegisterAsync_WhenValidationFails_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRequest() with { Name = "" };

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        result.Errors.First().Message.Should().Be(ErrorMessages.Name.Required);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        
        _repositoryMock
            .ExistsByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(true));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Be(ErrorMessages.Email.AlreadyExists);
        result.Errors.First().Metadata.Should().ContainKey("Code");
        result.Errors.First().Metadata["Code"].Should().Be(DomainErrors.Registrant.EmailAlreadyExists);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailCheckFails_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var repositoryError = new Error("Database connection failed");
        
        _repositoryMock
            .ExistsByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Fail<bool>(repositoryError));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Database connection failed");
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateFails_ReturnsFailure()
    {
        // Arrange
        var request = CreateValidRequest();
        var repositoryError = new Error("Create operation failed");
        
        _repositoryMock
            .ExistsByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(false));
        
        _repositoryMock
            .CreateAsync(Arg.Any<Registrant>(), Arg.Any<CancellationToken>())
            .Returns(Result.Fail<Registrant>(repositoryError));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Create operation failed");
    }

    [Fact]
    public async Task RegisterAsync_ShouldNormalizeEmail()
    {
        // Arrange
        var request = CreateValidRequest() with { Email = "TEST@EXAMPLE.COM" };
        
        _repositoryMock
            .ExistsByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(false));
        
        _repositoryMock
            .CreateAsync(Arg.Any<Registrant>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.Ok(callInfo.Arg<Registrant>()));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task RegisterAsync_ShouldTrimInputs()
    {
        // Arrange
        var request = CreateValidRequest() with 
        { 
            Name = "  John Doe  ",
            Email = " test@example.com ",
            Country = " us "
        };
        
        _repositoryMock
            .ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(false));
        
        Registrant? capturedRegistrant = null;
        _repositoryMock
            .CreateAsync(Arg.Do<Registrant>(r => capturedRegistrant = r), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.Ok(callInfo.Arg<Registrant>()));

        // Act
        await _sut.RegisterAsync(request);

        // Assert
        capturedRegistrant.Should().NotBeNull();
        capturedRegistrant!.Profile.Name.Should().Be("John Doe");
        capturedRegistrant.Profile.Email.Should().Be("test@example.com");
        capturedRegistrant.Profile.Country.Should().Be("US");
    }

    [Fact]
    public async Task RegisterAsync_ShouldDeduplicateLanguagesAndSkills()
    {
        // Arrange
        var request = CreateValidRequest() with 
        { 
            LanguagesSpoken = ["English", "English", "Persian"],
            ProfessionalSkills = ["Dev", "Dev", "Design"]
        };
        
        _repositoryMock
            .ExistsByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(false));
        
        Registrant? capturedRegistrant = null;
        _repositoryMock
            .CreateAsync(Arg.Do<Registrant>(r => capturedRegistrant = r), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.Ok(callInfo.Arg<Registrant>()));

        // Act
        await _sut.RegisterAsync(request);

        // Assert
        capturedRegistrant.Should().NotBeNull();
        capturedRegistrant!.Skills.LanguagesSpoken.Should().HaveCount(2);
        capturedRegistrant.Skills.ProfessionalSkills.Should().HaveCount(2);
    }

    [Fact]
    public async Task RegisterAsync_ShouldGenerateUniqueId()
    {
        // Arrange
        var request = CreateValidRequest();
        var capturedIds = new List<string>();
        
        _repositoryMock
            .ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(false));
        
        _repositoryMock
            .CreateAsync(Arg.Do<Registrant>(r => capturedIds.Add(r.Id)), Arg.Any<CancellationToken>())
            .Returns(callInfo => Result.Ok(callInfo.Arg<Registrant>()));

        // Act
        await _sut.RegisterAsync(request with { Email = "test1@test.com" });
        await _sut.RegisterAsync(request with { Email = "test2@test.com" });

        // Assert
        capturedIds.Should().HaveCount(2);
        capturedIds[0].Should().NotBe(capturedIds[1]);
    }

    [Fact]
    public async Task RegisterAsync_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var request = CreateValidRequest() with 
        { 
            Name = "", 
            Email = "invalid-email",
            LanguagesSpoken = []
        };

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task RegisterAsync_ShouldSupportCancellation()
    {
        // Arrange
        var request = CreateValidRequest();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        _repositoryMock
            .ExistsByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return await Task.FromResult(Result.Ok(false));
            });

        // Act
        var act = async () => await _sut.RegisterAsync(request, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenIdIsNull_ReturnsFailure()
    {
        // Act
        var result = await _sut.GetByIdAsync(null!);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(ErrorMessages.Registrant.IdRequired);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsEmpty_ReturnsFailure()
    {
        // Act
        var result = await _sut.GetByIdAsync("");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(ErrorMessages.Registrant.IdRequired);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsWhitespace_ReturnsFailure()
    {
        // Act
        var result = await _sut.GetByIdAsync("   ");

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(ErrorMessages.Registrant.IdRequired);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRegistrantExists_ReturnsRegistrant()
    {
        // Arrange
        var request = CreateValidRequest();
        var registrant = CreateRegistrant(request);
        
        _repositoryMock
            .GetByIdAsync(registrant.Id, Arg.Any<CancellationToken>())
            .Returns(Result.Ok(registrant));

        // Act
        var result = await _sut.GetByIdAsync(registrant.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(registrant);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepositoryFails_ReturnsFailure()
    {
        // Arrange
        var repositoryError = new Error("Not found")
            .WithMetadata("Code", DomainErrors.Registrant.NotFound);
        
        _repositoryMock
            .GetByIdAsync("non-existent-id", Arg.Any<CancellationToken>())
            .Returns(Result.Fail<Registrant>(repositoryError));

        // Act
        var result = await _sut.GetByIdAsync("non-existent-id");

        // Assert
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var registrant = CreateRegistrant(CreateValidRequest());
        var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        
        _repositoryMock
            .GetByIdAsync(Arg.Any<string>(), Arg.Do<CancellationToken>(t => capturedToken = t))
            .Returns(Result.Ok(registrant));

        // Act
        await _sut.GetByIdAsync(registrant.Id, cts.Token);

        // Assert
        capturedToken.Should().Be(cts.Token);
    }

    #endregion
}
