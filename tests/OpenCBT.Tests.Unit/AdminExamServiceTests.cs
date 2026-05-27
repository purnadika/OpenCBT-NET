using FluentAssertions;
using Moq;
using OpenCBT.Application.Services;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;
using System.Linq.Expressions;

namespace OpenCBT.Tests.Unit;

public class AdminExamServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly AdminExamService _adminService;

    public AdminExamServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _adminService = new AdminExamService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GradeEssayResponseAsync_ValidEssay_UpdatesPointsAndRecalculatesTotalScore()
    {
        // Arrange
        var responseId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var question = new Question
        {
            Id = questionId,
            Points = 10,
            Options = new List<AnswerOption>() // Essay has 0 options
        };

        var response = new StudentResponse
        {
            Id = responseId,
            ExamSessionId = sessionId,
            QuestionId = questionId,
            Question = question,
            EssayAnswer = "My written answer"
        };

        var session = new ExamSession
        {
            Id = sessionId,
            ExamId = examId,
            TotalScore = 0,
            Responses = new List<StudentResponse> { response }
        };

        _mockUnitOfWork.Setup(u => u.ExamSessions.FindAsync(It.IsAny<Expression<Func<ExamSession, bool>>>()))
                       .ReturnsAsync(new List<ExamSession> { session });

        _mockUnitOfWork.Setup(u => u.ExamSessions.GetSessionWithDetailsAsync(sessionId))
                       .ReturnsAsync(session);

        // Act
        await _adminService.GradeEssayResponseAsync(responseId, 8.5m, "Well written!");

        // Assert
        response.PointsObtained.Should().Be(8.5m);
        response.TeacherFeedback.Should().Be("Well written!");
        session.TotalScore.Should().Be(8.5m);

        _mockUnitOfWork.Verify(u => u.ExamSessions.Update(session), Times.Once);
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
