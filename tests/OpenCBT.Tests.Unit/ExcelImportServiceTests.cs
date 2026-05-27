using ClosedXML.Excel;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using OpenCBT.Application.Services;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace OpenCBT.Tests.Unit;

public class ExcelImportServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ExcelImportService _importService;

    public ExcelImportServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _importService = new ExcelImportService(_mockUserManager.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task ImportQuestionsAsync_ValidExcel_ParsesCorrectlyAndSaves()
    {
        // Arrange
        var examId = Guid.NewGuid();
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Questions");
        ws.Cell(1, 1).Value = "QuestionText";
        ws.Cell(1, 2).Value = "Points";
        ws.Cell(1, 3).Value = "QuestionType";
        ws.Cell(1, 4).Value = "OptionA";
        ws.Cell(1, 5).Value = "OptionB";
        ws.Cell(1, 6).Value = "OptionC";
        ws.Cell(1, 7).Value = "OptionD";
        ws.Cell(1, 8).Value = "CorrectOptionLetter";

        ws.Cell(2, 1).Value = "Paris capital?";
        ws.Cell(2, 2).Value = 10;
        ws.Cell(2, 3).Value = "MULTIPLE_CHOICE";
        ws.Cell(2, 4).Value = "Berlin";
        ws.Cell(2, 5).Value = "London";
        ws.Cell(2, 6).Value = "Paris";
        ws.Cell(2, 7).Value = "Madrid";
        ws.Cell(2, 8).Value = "C";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        _mockUnitOfWork.Setup(u => u.Questions.AddAsync(It.IsAny<Question>())).Returns(Task.CompletedTask);

        // Act
        var result = await _importService.ImportQuestionsAsync(examId, stream);

        // Assert
        result.Should().Be(1);
        _mockUnitOfWork.Verify(u => u.Questions.AddAsync(It.Is<Question>(q => 
            q.Text == "Paris capital?" && 
            q.Points == 10 && 
            q.Options.Count == 4
        )), Times.Once);
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
