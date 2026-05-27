using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Application.Services;

namespace OpenCBT.Web.Pages.Admin.Exams;

public class ImportQuestionsModel : PageModel
{
    private readonly ExcelTemplateService _templateService;
    private readonly ExcelImportService _importService;
    private readonly IAdminExamService _examService;

    public ImportQuestionsModel(ExcelTemplateService templateService, ExcelImportService importService, IAdminExamService examService)
    {
        _templateService = templateService;
        _importService = importService;
        _examService = examService;
    }

    public Guid ExamId { get; set; }
    public ExamDto Exam { get; set; } = null!;

    [BindProperty]
    public IFormFile UploadedFile { get; set; } = null!;

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid examId)
    {
        ExamId = examId;
        var exam = await _examService.GetExamByIdAsync(examId);
        if (exam == null) return NotFound();

        Exam = exam;
        return Page();
    }

    public IActionResult OnGetDownloadMultipleChoiceTemplate()
    {
        var templateBytes = _templateService.GenerateMultipleChoiceTemplate();
        return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MultipleChoiceTemplate.xlsx");
    }

    public IActionResult OnGetDownloadEssayTemplate()
    {
        var templateBytes = _templateService.GenerateEssayTemplate();
        return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EssayTemplate.xlsx");
    }

    public async Task<IActionResult> OnPostAsync(Guid examId)
    {
        ExamId = examId;
        var exam = await _examService.GetExamByIdAsync(examId);
        if (exam == null) return NotFound();
        Exam = exam;

        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ErrorMessage = "Please upload a valid Excel spreadsheet.";
            return Page();
        }

        try
        {
            using var stream = UploadedFile.OpenReadStream();
            int importedCount = await _importService.ImportQuestionsAsync(examId, stream);
            SuccessMessage = $"Successfully imported {importedCount} questions!";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Import failed: {ex.Message}";
        }

        return Page();
    }
}
