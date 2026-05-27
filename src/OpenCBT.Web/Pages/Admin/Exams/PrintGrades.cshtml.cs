using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Exams;

public class PrintGradesModel : PageModel
{
    private readonly IAdminExamService _adminExamService;

    public PrintGradesModel(IAdminExamService adminExamService)
    {
        _adminExamService = adminExamService;
    }

    public ExamAnalyticsDto Analytics { get; set; } = null!;
    
    // Sort students by Name by default, but allow score sorting
    public string SortBy { get; set; } = "name";

    public async Task<IActionResult> OnGetAsync(Guid examId, string sortBy = "name")
    {
        SortBy = sortBy;
        var analytics = await _adminExamService.GetExamAnalyticsAsync(examId);
        if (analytics == null) return NotFound();

        Analytics = analytics;

        // Apply sorting
        if (SortBy == "score_desc")
        {
            Analytics.StudentPerformances = Analytics.StudentPerformances
                .OrderByDescending(s => s.Score ?? 0)
                .ThenBy(s => s.FullName)
                .ToList();
        }
        else
        {
            Analytics.StudentPerformances = Analytics.StudentPerformances
                .OrderBy(s => s.FullName)
                .ToList();
        }

        return Page();
    }
}
