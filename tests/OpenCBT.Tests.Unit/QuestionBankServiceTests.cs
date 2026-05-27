using FluentAssertions;
using Moq;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Exceptions;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Tests.Unit;

public class QuestionBankServiceTests
{
    private readonly Mock<IQuestionBankService> _questionBankServiceMock;

    public QuestionBankServiceTests()
    {
        _questionBankServiceMock = new Mock<IQuestionBankService>();
    }

    [Fact]
    public async Task CreateQuestion_WithLessThanTwoOptions_ThrowsException()
    {
        // Arrange
        var request = new CreateQuestionDto
        {
            Text = "What is 2+2?",
            WeightPoint = 1,
            Options = new List<QuestionOptionDto>
            {
                new QuestionOptionDto { Text = "4", IsCorrect = true }
            } // Only 1 option
        };

        _questionBankServiceMock
            .Setup(x => x.CreateQuestionAsync(request))
            .ThrowsAsync(new ValidationException("A question must have at least two options."));

        var sut = _questionBankServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.CreateQuestionAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least two options*");
        _questionBankServiceMock.Verify(x => x.CreateQuestionAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateQuestion_WithNoCorrectAnswerDefined_ThrowsException()
    {
        // Arrange
        var request = new CreateQuestionDto
        {
            Text = "What is 2+2?",
            WeightPoint = 1,
            Options = new List<QuestionOptionDto>
            {
                new QuestionOptionDto { Text = "3", IsCorrect = false },
                new QuestionOptionDto { Text = "4", IsCorrect = false } // No correct answer
            }
        };

        _questionBankServiceMock
            .Setup(x => x.CreateQuestionAsync(request))
            .ThrowsAsync(new ValidationException("A question must have at least one correct option."));

        var sut = _questionBankServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.CreateQuestionAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one correct option*");
        _questionBankServiceMock.Verify(x => x.CreateQuestionAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateQuestion_WithNegativeWeightPoint_ThrowsException()
    {
        // Arrange
        var request = new CreateQuestionDto
        {
            Text = "What is 2+2?",
            WeightPoint = -5, // Negative point
            Options = new List<QuestionOptionDto>
            {
                new QuestionOptionDto { Text = "3", IsCorrect = false },
                new QuestionOptionDto { Text = "4", IsCorrect = true }
            }
        };

        _questionBankServiceMock
            .Setup(x => x.CreateQuestionAsync(request))
            .ThrowsAsync(new ValidationException("Weight point cannot be negative."));

        var sut = _questionBankServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.CreateQuestionAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*negative*");
        _questionBankServiceMock.Verify(x => x.CreateQuestionAsync(request), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestion_WhenUsedInActiveExam_ThrowsInvalidOperationException()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var request = new UpdateQuestionDto
        {
            Id = questionId,
            Text = "Updated question text",
            WeightPoint = 2,
            Options = new List<QuestionOptionDto>
            {
                new QuestionOptionDto { Text = "A", IsCorrect = true },
                new QuestionOptionDto { Text = "B", IsCorrect = false }
            }
        };

        _questionBankServiceMock
            .Setup(x => x.UpdateQuestionAsync(questionId, request))
            .ThrowsAsync(new InvalidOperationException("Cannot update question as it is part of an active exam."));

        var sut = _questionBankServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.UpdateQuestionAsync(questionId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*active exam*");
        _questionBankServiceMock.Verify(x => x.UpdateQuestionAsync(questionId, request), Times.Once);
    }

    [Fact]
    public async Task UploadQuestionImage_WithMaliciousExeRenamedToJpg_RejectsFile()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 0x4D, 0x5A }); // MZ header for exe
        var fileName = "image.jpg"; // Renamed
        var contentType = "image/jpeg";

        _questionBankServiceMock
            .Setup(x => x.UploadQuestionImageAsync(stream, fileName, contentType))
            .ThrowsAsync(new ArgumentException("Invalid image format or malicious file detected."));

        var sut = _questionBankServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.UploadQuestionImageAsync(stream, fileName, contentType);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*malicious*");
        _questionBankServiceMock.Verify(x => x.UploadQuestionImageAsync(stream, fileName, contentType), Times.Once);
    }

    [Fact]
    public async Task UploadQuestionImage_Exceeds2MBSizeLimit_ThrowsException()
    {
        // Arrange
        var stream = new MemoryStream(new byte[3 * 1024 * 1024]); // 3 MB stream
        var fileName = "large_image.jpg";
        var contentType = "image/jpeg";

        _questionBankServiceMock
            .Setup(x => x.UploadQuestionImageAsync(stream, fileName, contentType))
            .ThrowsAsync(new ArgumentException("File size exceeds the 2MB limit."));

        var sut = _questionBankServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.UploadQuestionImageAsync(stream, fileName, contentType);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*2MB limit*");
        _questionBankServiceMock.Verify(x => x.UploadQuestionImageAsync(stream, fileName, contentType), Times.Once);
    }
}
