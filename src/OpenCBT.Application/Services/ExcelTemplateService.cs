using ClosedXML.Excel;

namespace OpenCBT.Application.Services;

public class ExcelTemplateService
{
    public byte[] GenerateStudentTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Students");

        // Headers
        var headers = new[] { "FullName", "Email", "IdentifierNumber", "Password" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }

        // Mock Row
        ws.Cell(2, 1).Value = "John Doe";
        ws.Cell(2, 2).Value = "john.doe@example.com";
        ws.Cell(2, 3).Value = "NISN-998877";
        ws.Cell(2, 4).Value = "Password123!";

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateMultipleChoiceTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Multiple Choice Questions");

        // Headers
        var headers = new[] { "QuestionText", "Points", "OptionA", "OptionB", "OptionC", "OptionD", "CorrectOptionLetter" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }

        // Mock Row
        ws.Cell(2, 1).Value = "What is the capital of France?";
        ws.Cell(2, 2).Value = 10;
        ws.Cell(2, 3).Value = "Berlin";
        ws.Cell(2, 4).Value = "London";
        ws.Cell(2, 5).Value = "Paris";
        ws.Cell(2, 6).Value = "Madrid";
        ws.Cell(2, 7).Value = "C";

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GenerateEssayTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Essay Questions");

        // Headers
        var headers = new[] { "QuestionText", "Points" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }

        // Mock Row
        ws.Cell(2, 1).Value = "Explain the theory of relativity in your own words.";
        ws.Cell(2, 2).Value = 20;

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
