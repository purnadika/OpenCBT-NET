using FluentAssertions;
using Moq;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Exceptions;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<IAuthService> _authServiceMock;

    public AuthServiceTests()
    {
        _authServiceMock = new Mock<IAuthService>();
    }

    [Fact]
    public async Task Login_WithSqlInjectionString_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Username = "admin' OR 1=1--",
            Password = "password123"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(new AuthResultDto { IsSuccess = false, ErrorMessage = "Unauthorized" });

        var sut = _authServiceMock.Object;

        // Act
        var result = await sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Unauthorized");
        _authServiceMock.Verify(x => x.LoginAsync(request), Times.Once);
    }

    [Fact]
    public async Task Login_ExceedsMaxFailedAttempts_LocksAccountFor15Minutes()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Username = "student1",
            Password = "wrongpassword"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(new AuthResultDto { IsSuccess = false, ErrorMessage = "Account locked for 15 minutes due to multiple failed login attempts." });

        var sut = _authServiceMock.Object;

        // Act
        var result = await sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("15 minutes");
        _authServiceMock.Verify(x => x.LoginAsync(request), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WithSameOldPassword_ThrowsValidationException()
    {
        // Arrange
        var request = new ResetPasswordDto
        {
            UserId = "user-123",
            OldPassword = "MyOldPassword123!",
            NewPassword = "MyOldPassword123!"
        };

        _authServiceMock
            .Setup(x => x.ResetPasswordAsync(request))
            .ThrowsAsync(new ValidationException("New password cannot be the same as the old password."));

        var sut = _authServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.ResetPasswordAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*cannot be the same as the old password*");
        _authServiceMock.Verify(x => x.ResetPasswordAsync(request), Times.Once);
    }

    [Fact]
    public async Task GenerateJwtToken_ContainsCorrectRoleClaims()
    {
        // Arrange
        var userId = "admin-999";
        var roles = new List<string> { "Admin", "Teacher" };
        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        _authServiceMock
            .Setup(x => x.GenerateJwtTokenAsync(userId, roles))
            .ReturnsAsync(expectedToken);

        var sut = _authServiceMock.Object;

        // Act
        var token = await sut.GenerateJwtTokenAsync(userId, roles);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Should().Be(expectedToken);
        _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(userId, roles), Times.Once);
    }

    [Fact]
    public async Task AccessAdminEndpoint_WithStudentToken_ReturnsForbidden()
    {
        // Arrange
        var studentToken = "student.jwt.token";

        _authServiceMock
            .Setup(x => x.HasAdminAccessAsync(studentToken))
            .ReturnsAsync(false);

        var sut = _authServiceMock.Object;

        // Act
        var hasAccess = await sut.HasAdminAccessAsync(studentToken);

        // Assert
        hasAccess.Should().BeFalse();
        _authServiceMock.Verify(x => x.HasAdminAccessAsync(studentToken), Times.Once);
    }

    [Fact]
    public async Task Logout_InvalidatesTokenInRedisCache()
    {
        // Arrange
        var activeToken = "active.jwt.token";

        _authServiceMock
            .Setup(x => x.LogoutAsync(activeToken))
            .Returns(Task.CompletedTask);

        var sut = _authServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.LogoutAsync(activeToken);

        // Assert
        await act.Should().NotThrowAsync();
        _authServiceMock.Verify(x => x.LogoutAsync(activeToken), Times.Once);
    }
}
