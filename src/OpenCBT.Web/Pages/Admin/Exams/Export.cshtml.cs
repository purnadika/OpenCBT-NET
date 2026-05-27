using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Exams;

public class ExportModel : PageModel
{
    private readonly IReportService _reportService;

    public ExportModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            var excelBytes = await _reportService.GenerateExcelReportAsync(id);
            var fileName = $"Exam_Grades_{id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to export grades: {ex.Message}";
            return RedirectToPage("./Index");
        }
    }
}
