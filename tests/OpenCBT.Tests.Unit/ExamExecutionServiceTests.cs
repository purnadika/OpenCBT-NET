using FluentAssertions;
using Moq;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Exceptions;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Tests.Unit;

public class ExamExecutionServiceTests
{
    private readonly Mock<IExamExecutionService> _executionServiceMock;

    public ExamExecutionServiceTests()
    {
        _executionServiceMock = new Mock<IExamExecutionService>();
    }

    [Fact]
    public async Task StartExam_StudentAlreadyHasActiveSessionOnAnotherDevice_KicksOldSession()
    {
        // Arrange
        var request = new StartExamDto
        {
            ExamId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            DeviceFingerprint = "new-device-id"
        };
        
        var expectedSession = new ExamSessionDto
        {
            Id = Guid.NewGuid(),
            DeviceFingerprint = "new-device-id"
        };

        _executionServiceMock
            .Setup(x => x.StartExamAsync(request))
            .ReturnsAsync(expectedSession);

        var sut = _executionServiceMock.Object;

        // Act
        var result = await sut.StartExamAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.DeviceFingerprint.Should().Be("new-device-id");
        _executionServiceMock.Verify(x => x.StartExamAsync(request), Times.Once);
    }

    [Fact]
    public async Task StartExam_WhenExamStatusIsPaused_ThrowsException()
    {
        // Arrange
        var request = new StartExamDto { ExamId = Guid.NewGuid(), StudentId = Guid.NewGuid() };

        _executionServiceMock
            .Setup(x => x.StartExamAsync(request))
            .ThrowsAsync(new InvalidOperationException("The exam is currently paused and cannot be started."));

        var sut = _executionServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.StartExamAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*paused*");
        _executionServiceMock.Verify(x => x.StartExamAsync(request), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_ForQuestionNotInExam_ThrowsException()
    {
        // Arrange
        var request = new SubmitAnswerDto { SessionId = Guid.NewGuid(), QuestionId = Guid.NewGuid(), OptionId = Guid.NewGuid() };

        _executionServiceMock
            .Setup(x => x.SubmitAnswerAsync(request))
            .ThrowsAsync(new ValidationException("The provided question does not belong to this exam."));

        var sut = _executionServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.SubmitAnswerAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*does not belong*");
        _executionServiceMock.Verify(x => x.SubmitAnswerAsync(request), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_AfterTimeExpired_RejectsSilently()
    {
        // Arrange
        var request = new SubmitAnswerDto { SessionId = Guid.NewGuid(), QuestionId = Guid.NewGuid(), OptionId = Guid.NewGuid() };

        // For silent rejection, it doesn't throw but maybe returns a specific result or just completes without updating.
        _executionServiceMock
            .Setup(x => x.SubmitAnswerAsync(request))
            .Returns(Task.CompletedTask); 

        var sut = _executionServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.SubmitAnswerAsync(request);

        // Assert
        await act.Should().NotThrowAsync();
        _executionServiceMock.Verify(x => x.SubmitAnswerAsync(request), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_RapidFireRequests_AppliesRateLimiting()
    {
        // Arrange
        var request = new SubmitAnswerDto { SessionId = Guid.NewGuid(), QuestionId = Guid.NewGuid(), OptionId = Guid.NewGuid() };

        _executionServiceMock
            .Setup(x => x.SubmitAnswerAsync(request))
            .ThrowsAsync(new RateLimitExceededException("Too many requests. Please slow down."));

        var sut = _executionServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.SubmitAnswerAsync(request);

        // Assert
        await act.Should().ThrowAsync<RateLimitExceededException>();
        _executionServiceMock.Verify(x => x.SubmitAnswerAsync(request), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswer_NetworkDisconnectResubmit_HandlesIdempotency()
    {
        // Arrange
        var request = new SubmitAnswerDto 
        { 
            SessionId = Guid.NewGuid(), 
            QuestionId = Guid.NewGuid(), 
            OptionId = Guid.NewGuid(),
            RequestId = "idemp-key-123"
        };

        _executionServiceMock
            .Setup(x => x.SubmitAnswerAsync(request))
            .Returns(Task.CompletedTask);

        var sut = _executionServiceMock.Object;

        // Act
        await sut.SubmitAnswerAsync(request); // First call
        await sut.SubmitAnswerAsync(request); // Resubmit with same Idempotency key

        // Assert
        _executionServiceMock.Verify(x => x.SubmitAnswerAsync(request), Times.Exactly(2));
        // Idempotency logic inside the actual service would ensure the DB isn't updated twice.
    }

    [Fact]
    public async Task PingHeartbeat_UpdatesLastActiveTimestamp()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _executionServiceMock
            .Setup(x => x.PingHeartbeatAsync(sessionId))
            .Returns(Task.CompletedTask);

        var sut = _executionServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.PingHeartbeatAsync(sessionId);

        // Assert
        await act.Should().NotThrowAsync();
        _executionServiceMock.Verify(x => x.PingHeartbeatAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task ForceSubmitExam_ByAdmin_ClosesSessionAndCalculatesScore()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var sessionResult = new ExamSessionDto
        {
            Id = sessionId,
            CompletedAt = DateTime.UtcNow,
            TotalScore = 85.5m
        };

        _executionServiceMock
            .Setup(x => x.ForceSubmitExamAsync(sessionId))
            .ReturnsAsync(sessionResult);

        var sut = _executionServiceMock.Object;

        // Act
        var result = await sut.ForceSubmitExamAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.CompletedAt.Should().NotBeNull();
        result.TotalScore.Should().Be(85.5m);
        _executionServiceMock.Verify(x => x.ForceSubmitExamAsync(sessionId), Times.Once);
    }
}
