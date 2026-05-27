using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using OpenCBT.Application.Interfaces;
using OpenCBT.Application.Services;
using OpenCBT.Application.DTOs;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;
using OpenCBT.Application.Exceptions;

namespace OpenCBT.Tests.Unit;

public class ExamServiceTests
{
    private readonly Mock<IExamRepository> _examRepositoryMock;
    private readonly Mock<IExamSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IGenericRepository<Question>> _questionRepositoryMock;
    private readonly Mock<IGenericRepository<AnswerOption>> _answerOptionRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IExamService _examService;

    public ExamServiceTests()
    {
        _examRepositoryMock = new Mock<IExamRepository>();
        _sessionRepositoryMock = new Mock<IExamSessionRepository>();
        _questionRepositoryMock = new Mock<IGenericRepository<Question>>();
        _answerOptionRepositoryMock = new Mock<IGenericRepository<AnswerOption>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _unitOfWorkMock.Setup(u => u.Exams).Returns(_examRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ExamSessions).Returns(_sessionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Questions).Returns(_questionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.AnswerOptions).Returns(_answerOptionRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        _examService = new ExamService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task StartExam_WithInvalidToken_ThrowsValidationException()
    {
        // Arrange
        var examId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var exam = new Exam 
        { 
            Id = examId, 
            TokenRequired = true, 
            CurrentToken = "SECRET",
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            EndTime = DateTime.UtcNow.AddMinutes(10),
            IsActive = true
        };

        _examRepositoryMock.Setup(r => r.GetByIdAsync(examId)).ReturnsAsync(exam);

        var request = new StartExamRequestDto 
        { 
            ExamId = examId, 
            UserId = userId, 
            Token = "WRONG_TOKEN" 
        };

        // Act
        Func<Task> action = async () => await _examService.StartExamAsync(request);

        // Assert
        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid token*");
    }

    [Fact]
    public async Task StartExam_OutsideTimeWindow_ThrowsValidationException()
    {
        // Arrange
        var examId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var exam = new Exam 
        { 
            Id = examId, 
            TokenRequired = false,
            StartTime = DateTime.UtcNow.AddMinutes(10), // Exam hasn't started yet
            EndTime = DateTime.UtcNow.AddMinutes(60),
            IsActive = true
        };

        _examRepositoryMock.Setup(r => r.GetByIdAsync(examId)).ReturnsAsync(exam);

        var request = new StartExamRequestDto 
        { 
            ExamId = examId, 
            UserId = userId 
        };

        // Act
        Func<Task> action = async () => await _examService.StartExamAsync(request);

        // Assert
        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("*outside the allowed time window*");
    }

    [Fact]
    public async Task StartExam_ValidRequest_ReturnsExamSessionDto()
    {
        // Arrange
        var examId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var exam = new Exam 
        { 
            Id = examId, 
            TokenRequired = true, 
            CurrentToken = "SECRET",
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            EndTime = DateTime.UtcNow.AddMinutes(10),
            IsActive = true
        };

        _examRepositoryMock.Setup(r => r.GetByIdAsync(examId)).ReturnsAsync(exam);

        var request = new StartExamRequestDto 
        { 
            ExamId = examId, 
            UserId = userId, 
            Token = "SECRET" 
        };

        // Act
        var result = await _examService.StartExamAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ExamId.Should().Be(examId);
        result.UserId.Should().Be(userId);
        result.Status.Should().Be("InProgress");
        
        _sessionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ExamSession>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswerAsync_SessionAlreadyCompleted_ThrowsValidationException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ExamSession 
        { 
            Id = sessionId,
            CompletedAt = DateTime.UtcNow 
        };

        _sessionRepositoryMock.Setup(r => r.GetSessionWithDetailsAsync(sessionId)).ReturnsAsync(session);

        // Act
        Func<Task> action = async () => await _examService.SubmitAnswerAsync(sessionId, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("*already completed*");
    }

    [Fact]
    public async Task SubmitAnswerAsync_ValidRequest_UpdatesResponse()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var session = new ExamSession 
        { 
            Id = sessionId,
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            Exam = new Exam { DurationMinutes = 60 }
        };

        _sessionRepositoryMock.Setup(r => r.GetSessionWithDetailsAsync(sessionId)).ReturnsAsync(session);

        // Act
        await _examService.SubmitAnswerAsync(sessionId, questionId, optionId);

        // Assert
        session.Responses.Should().ContainSingle(r => r.QuestionId == questionId && r.SelectedAnswerOptionId == optionId);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CompleteExamAsync_ValidSession_CalculatesScoreAndCompletes()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var questionId1 = Guid.NewGuid();
        var questionId2 = Guid.NewGuid();
        var correctOptionId1 = Guid.NewGuid();
        var wrongOptionId2 = Guid.NewGuid();
        
        var session = new ExamSession 
        { 
            Id = sessionId,
            Responses = new List<StudentResponse>
            {
                new StudentResponse { QuestionId = questionId1, SelectedAnswerOptionId = correctOptionId1 },
                new StudentResponse { QuestionId = questionId2, SelectedAnswerOptionId = wrongOptionId2 }
            }
        };

        var correctOptions = new List<AnswerOption>
        {
            new AnswerOption { QuestionId = questionId1, Id = correctOptionId1, IsCorrect = true },
            new AnswerOption { QuestionId = questionId2, Id = Guid.NewGuid(), IsCorrect = true }
        };

        var questions = new List<Question>
        {
            new Question { Id = questionId1, Points = 10 },
            new Question { Id = questionId2, Points = 10 }
        };

        _sessionRepositoryMock.Setup(r => r.GetSessionWithDetailsAsync(sessionId)).ReturnsAsync(session);
        _unitOfWorkMock.Setup(u => u.AnswerOptions.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<AnswerOption, bool>>>()))
            .ReturnsAsync(correctOptions);
        _unitOfWorkMock.Setup(u => u.Questions.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Question, bool>>>()))
            .ReturnsAsync(questions);

        // Act
        var result = await _examService.CompleteExamAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.TotalScore.Should().Be(10); // Only Q1 is correct
        session.CompletedAt.Should().NotBeNull();
        session.TotalScore.Should().Be(10);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
