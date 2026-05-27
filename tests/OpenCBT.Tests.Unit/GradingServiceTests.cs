using FluentAssertions;
using Moq;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Tests.Unit;

public class GradingServiceTests
{
    private readonly Mock<IGradingService> _gradingServiceMock;

    public GradingServiceTests()
    {
        _gradingServiceMock = new Mock<IGradingService>();
    }

    [Fact]
    public async Task CalculateScore_AllCorrect_Returns100()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedResult = new GradingResultDto
        {
            SessionId = sessionId,
            Score = 100m,
            CorrectAnswers = 50,
            IncorrectAnswers = 0,
            UnansweredQuestions = 0
        };

        _gradingServiceMock
            .Setup(x => x.CalculateScoreAsync(sessionId))
            .ReturnsAsync(expectedResult);

        var sut = _gradingServiceMock.Object;

        // Act
        var result = await sut.CalculateScoreAsync(sessionId);

        // Assert
        result.Score.Should().Be(100m);
        result.CorrectAnswers.Should().BeGreaterThan(0);
        result.IncorrectAnswers.Should().Be(0);
        _gradingServiceMock.Verify(x => x.CalculateScoreAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task CalculateScore_AllWrong_Returns0()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedResult = new GradingResultDto
        {
            SessionId = sessionId,
            Score = 0m,
            CorrectAnswers = 0,
            IncorrectAnswers = 50,
            UnansweredQuestions = 0
        };

        _gradingServiceMock
            .Setup(x => x.CalculateScoreAsync(sessionId))
            .ReturnsAsync(expectedResult);

        var sut = _gradingServiceMock.Object;

        // Act
        var result = await sut.CalculateScoreAsync(sessionId);

        // Assert
        result.Score.Should().Be(0m);
        result.CorrectAnswers.Should().Be(0);
        result.IncorrectAnswers.Should().BeGreaterThan(0);
        _gradingServiceMock.Verify(x => x.CalculateScoreAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task CalculateScore_SomeUnanswered_CalculatesOnlyAnswered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedResult = new GradingResultDto
        {
            SessionId = sessionId,
            Score = 50m,
            CorrectAnswers = 25,
            IncorrectAnswers = 15,
            UnansweredQuestions = 10
        };

        _gradingServiceMock
            .Setup(x => x.CalculateScoreAsync(sessionId))
            .ReturnsAsync(expectedResult);

        var sut = _gradingServiceMock.Object;

        // Act
        var result = await sut.CalculateScoreAsync(sessionId);

        // Assert
        result.UnansweredQuestions.Should().Be(10);
        result.Score.Should().Be(50m);
        _gradingServiceMock.Verify(x => x.CalculateScoreAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task CalculateScore_WithDecimalWeights_RoundsToTwoDecimalPlaces()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedResult = new GradingResultDto
        {
            SessionId = sessionId,
            Score = 83.33m // 100/3 * 2.5 rounded
        };

        _gradingServiceMock
            .Setup(x => x.CalculateScoreAsync(sessionId))
            .ReturnsAsync(expectedResult);

        var sut = _gradingServiceMock.Object;

        // Act
        var result = await sut.CalculateScoreAsync(sessionId);

        // Assert
        result.Score.Should().Be(83.33m);
        _gradingServiceMock.Verify(x => x.CalculateScoreAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task RecalculateScore_WhenBonusQuestionTriggered_UpdatesScoreCorrectly()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var bonusAppliedResult = new GradingResultDto
        {
            SessionId = sessionId,
            Score = 95m // Was maybe 90 before
        };

        _gradingServiceMock
            .Setup(x => x.RecalculateScoreAsync(sessionId, true))
            .ReturnsAsync(bonusAppliedResult);

        var sut = _gradingServiceMock.Object;

        // Act
        var result = await sut.RecalculateScoreAsync(sessionId, applyBonus: true);

        // Assert
        result.Score.Should().Be(95m);
        _gradingServiceMock.Verify(x => x.RecalculateScoreAsync(sessionId, true), Times.Once);
    }

    [Fact]
    public async Task GetExamAnalytics_CalculatesAverageHighestAndLowestCorrectly()
    {
        // Arrange
        var examId = Guid.NewGuid();
        var expectedAnalytics = new ExamAnalyticsDto
        {
            ExamId = examId,
            AverageScore = 75.5m,
            HighestScore = 100m,
            LowestScore = 32.5m,
            TotalParticipants = 150
        };

        _gradingServiceMock
            .Setup(x => x.GetExamAnalyticsAsync(examId))
            .ReturnsAsync(expectedAnalytics);

        var sut = _gradingServiceMock.Object;

        // Act
        var result = await sut.GetExamAnalyticsAsync(examId);

        // Assert
        result.AverageScore.Should().Be(75.5m);
        result.HighestScore.Should().Be(100m);
        result.LowestScore.Should().Be(32.5m);
        result.TotalParticipants.Should().Be(150);
        _gradingServiceMock.Verify(x => x.GetExamAnalyticsAsync(examId), Times.Once);
    }
}
