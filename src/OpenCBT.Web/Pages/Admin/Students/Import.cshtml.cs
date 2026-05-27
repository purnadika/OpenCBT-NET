using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.Services;

namespace OpenCBT.Web.Pages.Admin.Students;

public class ImportModel : PageModel
{
    private readonly ExcelTemplateService _templateService;
    private readonly ExcelImportService _importService;

    public ImportModel(ExcelTemplateService templateService, ExcelImportService importService)
    {
        _templateService = templateService;
        _importService = importService;
    }

    [BindProperty]
    public IFormFile UploadedFile { get; set; } = null!;

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var templateBytes = _templateService.GenerateStudentTemplate();
        return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentImportTemplate.xlsx");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ErrorMessage = "Please upload a valid Excel spreadsheet.";
            return Page();
        }

        try
        {
            using var stream = UploadedFile.OpenReadStream();
            int importedCount = await _importService.ImportStudentsAsync(stream);
            SuccessMessage = $"Successfully imported {importedCount} students!";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Import failed: {ex.Message}";
        }

        return Page();
    }
}
