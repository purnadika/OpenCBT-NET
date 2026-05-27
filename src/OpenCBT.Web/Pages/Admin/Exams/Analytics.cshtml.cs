using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Exams;

public class AnalyticsModel : PageModel
{
    private readonly IAdminExamService _examService;

    public AnalyticsModel(IAdminExamService examService)
    {
        _examService = examService;
    }

    public ExamAnalyticsDto Analytics { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid examId)
    {
        try
        {
            Analytics = await _examService.GetExamAnalyticsAsync(examId);
            return Page();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
