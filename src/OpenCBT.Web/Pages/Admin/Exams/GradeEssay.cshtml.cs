using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Exams;

public class GradeEssayModel : PageModel
{
    private readonly IAdminExamService _examService;

    public GradeEssayModel(IAdminExamService examService)
    {
        _examService = examService;
    }

    public ExamSessionDetailsDto SessionDetails { get; set; } = null!;
    
    [BindProperty]
    public Guid ResponseId { get; set; }

    [BindProperty]
    public decimal PointsObtained { get; set; }

    [BindProperty]
    public string TeacherFeedback { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid sessionId)
    {
        var details = await _examService.GetSessionDetailsAsync(sessionId);
        if (details == null) return NotFound();

        SessionDetails = details;
        return Page();
    }

    public async Task<IActionResult> OnPostGradeAsync(Guid sessionId)
    {
        try
        {
            await _examService.GradeEssayResponseAsync(ResponseId, PointsObtained, TeacherFeedback ?? string.Empty);
            TempData["SuccessMessage"] = "Essay answer graded successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Grading failed: {ex.Message}";
        }

        return RedirectToPage(new { sessionId });
    }
}
