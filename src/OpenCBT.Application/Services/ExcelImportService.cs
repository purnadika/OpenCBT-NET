using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using OpenCBT.Domain.Entities;
using OpenCBT.Domain.Interfaces;

namespace OpenCBT.Application.Services;

public class ExcelImportService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public ExcelImportService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> ImportStudentsAsync(Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheet(1);
        var rows = ws.RangeUsed().RowsUsed().Skip(1); // Skip header

        int successCount = 0;
        foreach (var row in rows)
        {
            var fullName = row.Cell(1).Value.ToString().Trim();
            var email = row.Cell(2).Value.ToString().Trim();
            var identifierNumber = row.Cell(3).Value.ToString().Trim();
            var password = row.Cell(4).Value.ToString().Trim();

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email)) continue;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                IdentifierNumber = identifierNumber,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, string.IsNullOrEmpty(password) ? "Student123!" : password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");
                successCount++;
            }
        }

        return successCount;
    }

    public async Task<int> ImportQuestionsAsync(Guid examId, Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var ws = workbook.Worksheet(1);
        
        var headerRow = ws.Row(1);
        int colCount = ws.ColumnsUsed().Count();

        int textCol = 1;
        int pointsCol = 2;
        int optACol = -1;
        int optBCol = -1;
        int optCCol = -1;
        int optDCol = -1;
        int correctCol = -1;

        for (int c = 1; c <= colCount; c++)
        {
            var headerVal = headerRow.Cell(c).Value.ToString().Trim().ToUpperInvariant();
            if (headerVal == "QUESTIONTEXT" || headerVal == "QUESTION TEXT") textCol = c;
            else if (headerVal == "POINTS") pointsCol = c;
            else if (headerVal == "OPTIONA" || headerVal == "OPTION A") optACol = c;
            else if (headerVal == "OPTIONB" || headerVal == "OPTION B") optBCol = c;
            else if (headerVal == "OPTIONC" || headerVal == "OPTION C") optCCol = c;
            else if (headerVal == "OPTIOND" || headerVal == "OPTION D") optDCol = c;
            else if (headerVal == "CORRECTOPTIONLETTER" || headerVal == "CORRECT OPTION" || headerVal == "CORRECT LETTER" || headerVal == "CORRECTOPTION") correctCol = c;
        }

        var rows = ws.RangeUsed().RowsUsed().Skip(1); // Skip header
        int questionCount = 0;
        int orderIndex = 1;

        foreach (var row in rows)
        {
            var text = row.Cell(textCol).Value.ToString().Trim();
            var pointsStr = pointsCol > 0 ? row.Cell(pointsCol).Value.ToString().Trim() : "";

            if (string.IsNullOrEmpty(text)) continue;

            decimal.TryParse(pointsStr, out decimal points);
            if (points <= 0) points = 10;

            var question = new Question
            {
                ExamId = examId,
                Text = text,
                Points = points,
                OrderIndex = orderIndex++
            };

            // Check if Multiple Choice (we have optACol and option data)
            if (optACol > 0 && optBCol > 0)
            {
                var optA = row.Cell(optACol).Value.ToString().Trim();
                var optB = row.Cell(optBCol).Value.ToString().Trim();
                var optC = optCCol > 0 ? row.Cell(optCCol).Value.ToString().Trim() : "";
                var optD = optDCol > 0 ? row.Cell(optDCol).Value.ToString().Trim() : "";
                var correctLetter = correctCol > 0 ? row.Cell(correctCol).Value.ToString().Trim().ToUpperInvariant() : "";

                if (!string.IsNullOrEmpty(optA))
                {
                    question.Options.Add(new AnswerOption { Text = optA, OrderIndex = 1, IsCorrect = correctLetter == "A" });
                }
                if (!string.IsNullOrEmpty(optB))
                {
                    question.Options.Add(new AnswerOption { Text = optB, OrderIndex = 2, IsCorrect = correctLetter == "B" });
                }
                if (!string.IsNullOrEmpty(optC))
                {
                    question.Options.Add(new AnswerOption { Text = optC, OrderIndex = 3, IsCorrect = correctLetter == "C" });
                }
                if (!string.IsNullOrEmpty(optD))
                {
                    question.Options.Add(new AnswerOption { Text = optD, OrderIndex = 4, IsCorrect = correctLetter == "D" });
                }
            }

            await _unitOfWork.Questions.AddAsync(question);
            questionCount++;
        }

        await _unitOfWork.CompleteAsync();
        return questionCount;
    }
}
