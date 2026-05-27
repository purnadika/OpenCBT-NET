using FluentAssertions;
using Moq;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Exceptions;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Tests.Unit;

public class ExamSchedulingServiceTests
{
    private readonly Mock<IExamSchedulingService> _schedulingServiceMock;

    public ExamSchedulingServiceTests()
    {
        _schedulingServiceMock = new Mock<IExamSchedulingService>();
    }

    [Fact]
    public async Task CreateExam_WithEndTimeBeforeStartTime_ThrowsException()
    {
        // Arrange
        var request = new CreateExamScheduleDto
        {
            Title = "Math Final",
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(1), // End is before Start
            DurationMinutes = 60,
            AssignedQuestionIds = new List<Guid> { Guid.NewGuid() }
        };

        _schedulingServiceMock
            .Setup(x => x.CreateExamAsync(request))
            .ThrowsAsync(new ValidationException("EndTime must be after StartTime."));

        var sut = _schedulingServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.CreateExamAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*EndTime must be after StartTime*");
        _schedulingServiceMock.Verify(x => x.CreateExamAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateExam_WithDurationLongerThanTimeWindow_ThrowsException()
    {
        // Arrange
        var request = new CreateExamScheduleDto
        {
            Title = "Physics Midterm",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2), // 1 hour window
            DurationMinutes = 120, // 2 hours duration
            AssignedQuestionIds = new List<Guid> { Guid.NewGuid() }
        };

        _schedulingServiceMock
            .Setup(x => x.CreateExamAsync(request))
            .ThrowsAsync(new ValidationException("Duration exceeds the available time window."));

        var sut = _schedulingServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.CreateExamAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*exceeds the available time window*");
        _schedulingServiceMock.Verify(x => x.CreateExamAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateExam_WithZeroQuestionsAssigned_ThrowsException()
    {
        // Arrange
        var request = new CreateExamScheduleDto
        {
            Title = "Empty Exam",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2),
            DurationMinutes = 60,
            AssignedQuestionIds = new List<Guid>() // Zero questions
        };

        _schedulingServiceMock
            .Setup(x => x.CreateExamAsync(request))
            .ThrowsAsync(new ValidationException("An exam must have at least one assigned question."));

        var sut = _schedulingServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.CreateExamAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one assigned question*");
        _schedulingServiceMock.Verify(x => x.CreateExamAsync(request), Times.Once);
    }

    [Fact]
    public async Task GenerateToken_ForPastExam_ThrowsException()
    {
        // Arrange
        var examId = Guid.NewGuid();

        _schedulingServiceMock
            .Setup(x => x.GenerateTokenAsync(examId))
            .ThrowsAsync(new InvalidOperationException("Cannot generate token for a past exam."));

        var sut = _schedulingServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.GenerateTokenAsync(examId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*past exam*");
        _schedulingServiceMock.Verify(x => x.GenerateTokenAsync(examId), Times.Once);
    }

    [Fact]
    public async Task RefreshExamToken_WhenExamIsFinished_ThrowsException()
    {
        // Arrange
        var examId = Guid.NewGuid();

        _schedulingServiceMock
            .Setup(x => x.RefreshExamTokenAsync(examId))
            .ThrowsAsync(new InvalidOperationException("Cannot refresh token for a finished exam."));

        var sut = _schedulingServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.RefreshExamTokenAsync(examId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*finished exam*");
        _schedulingServiceMock.Verify(x => x.RefreshExamTokenAsync(examId), Times.Once);
    }

    [Fact]
    public async Task DeleteExam_WithExistingStudentSessions_ThrowsException()
    {
        // Arrange
        var examId = Guid.NewGuid();

        _schedulingServiceMock
            .Setup(x => x.DeleteExamAsync(examId))
            .ThrowsAsync(new InvalidOperationException("Cannot delete exam because there are active or completed student sessions."));

        var sut = _schedulingServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.DeleteExamAsync(examId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*student sessions*");
        _schedulingServiceMock.Verify(x => x.DeleteExamAsync(examId), Times.Once);
    }
}
