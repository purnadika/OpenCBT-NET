using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Application.Interfaces;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateExcelReportAsync(Guid examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null) throw new KeyNotFoundException("Exam not found");

        var sessions = await _context.ExamSessions
            .Include(s => s.User)
            .Where(s => s.ExamId == examId && s.CompletedAt != null)
            .OrderByDescending(s => s.TotalScore)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Grades Report");

        // Header Formatting
        worksheet.Cell("A1").Value = $"Exam Grades: {exam.Title}";
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 16;
        worksheet.Range("A1:E1").Merge();

        worksheet.Cell("A2").Value = $"Exported on: {DateTime.UtcNow:g} (UTC)";
        worksheet.Cell("A2").Style.Font.Italic = true;
        worksheet.Range("A2:E2").Merge();

        // Table Headers
        var headers = new[] { "Student Name", "ID Number (NISN)", "Started At", "Completed At", "Score" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e40af");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data Rows
        int currentRow = 5;
        foreach (var s in sessions)
        {
            worksheet.Cell(currentRow, 1).Value = s.User?.FullName ?? "Unknown Student";
            worksheet.Cell(currentRow, 2).Value = s.User?.IdentifierNumber ?? "N/A";
            worksheet.Cell(currentRow, 3).Value = s.StartedAt.ToString("g");
            worksheet.Cell(currentRow, 4).Value = s.CompletedAt?.ToString("g") ?? "N/A";
            worksheet.Cell(currentRow, 5).Value = s.TotalScore ?? 0;
            currentRow++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
