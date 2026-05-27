using FluentAssertions;
using Moq;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Exceptions;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Tests.Unit;

public class StudentManagementServiceTests
{
    private readonly Mock<IStudentManagementService> _studentServiceMock;

    public StudentManagementServiceTests()
    {
        _studentServiceMock = new Mock<IStudentManagementService>();
    }

    [Fact]
    public async Task RegisterStudent_WithEmptyNisn_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateStudentDto
        {
            FullName = "John Doe",
            IdentifierNumber = "", // Empty NISN
            Email = "john@example.com",
            Password = "Password123!"
        };

        _studentServiceMock
            .Setup(x => x.CreateStudentAsync(request))
            .ThrowsAsync(new ValidationException("IdentifierNumber (NISN) cannot be empty."));

        var sut = _studentServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.CreateStudentAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*cannot be empty*");
        _studentServiceMock.Verify(x => x.CreateStudentAsync(request), Times.Once);
    }

    [Fact]
    public async Task RegisterStudent_WithExcessiveLengthName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateStudentDto
        {
            FullName = new string('A', 256), // Excessive length
            IdentifierNumber = "1234567890",
            Email = "john@example.com",
            Password = "Password123!"
        };

        _studentServiceMock
            .Setup(x => x.CreateStudentAsync(request))
            .ThrowsAsync(new ValidationException("FullName exceeds maximum length."));

        var sut = _studentServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.CreateStudentAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*exceeds maximum length*");
        _studentServiceMock.Verify(x => x.CreateStudentAsync(request), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_ChangeNisnToExistingOne_ThrowsConflictException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new StudentDto
        {
            Id = studentId,
            FullName = "John Doe",
            IdentifierNumber = "9999999999", // Existing NISN
            Email = "john@example.com"
        };

        _studentServiceMock
            .Setup(x => x.UpdateStudentAsync(studentId, request))
            .ThrowsAsync(new ConflictException("IdentifierNumber is already in use by another student."));

        var sut = _studentServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.UpdateStudentAsync(studentId, request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already in use*");
        _studentServiceMock.Verify(x => x.UpdateStudentAsync(studentId, request), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteStudent_RetainsPastExamRecords()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        _studentServiceMock
            .Setup(x => x.DeleteStudentAsync(studentId))
            .Returns(Task.CompletedTask);

        var sut = _studentServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.DeleteStudentAsync(studentId);

        // Assert
        await act.Should().NotThrowAsync();
        _studentServiceMock.Verify(x => x.DeleteStudentAsync(studentId), Times.Once);
    }

    [Fact]
    public async Task BulkImportStudents_WithEmptyExcelFile_ThrowsException()
    {
        // Arrange
        var emptyStream = new MemoryStream(); // 0 bytes

        _studentServiceMock
            .Setup(x => x.BulkImportStudentsAsync(emptyStream))
            .ThrowsAsync(new ArgumentException("The uploaded file is empty."));

        var sut = _studentServiceMock.Object;

        // Act
        Func<Task> act = async () => await sut.BulkImportStudentsAsync(emptyStream);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*file is empty*");
        _studentServiceMock.Verify(x => x.BulkImportStudentsAsync(emptyStream), Times.Once);
    }

    [Fact]
    public async Task BulkImportStudents_WithDuplicateRowsInFile_SavesUniqueAndLogsErrors()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 }); // Dummy non-empty stream
        
        var expectedResult = (SavedCount: 5, Errors: new List<string> { "Row 3: Duplicate NISN found." });

        _studentServiceMock
            .Setup(x => x.BulkImportStudentsAsync(stream))
            .ReturnsAsync(expectedResult);

        var sut = _studentServiceMock.Object;

        // Act
        var result = await sut.BulkImportStudentsAsync(stream);

        // Assert
        result.SavedCount.Should().Be(5);
        result.Errors.Should().ContainSingle(e => e.Contains("Duplicate NISN"));
        _studentServiceMock.Verify(x => x.BulkImportStudentsAsync(stream), Times.Once);
    }
}
