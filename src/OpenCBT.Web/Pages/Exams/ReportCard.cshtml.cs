using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages.Exams;

public class ReportCardModel : PageModel
{
    private readonly IExamService _examService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReportCardModel(IExamService examService, UserManager<ApplicationUser> userManager)
    {
        _examService = examService;
        _userManager = userManager;
    }

    public ExamSessionDetailsDto ReportDetails { get; set; } = null!;
    public decimal MaxScore { get; set; }
    public decimal Percentage { get; set; }
    public bool IsPassed { get; set; }
    
    // Fixed Passing threshold, could be moved to configuration/exam domain later
    private const decimal PassingPercentageThreshold = 60.0m;

    public async Task<IActionResult> OnGetAsync(Guid sessionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            var details = await _examService.GetStudentSessionReviewAsync(sessionId, user.Id);
            if (details == null) return NotFound();

            ReportDetails = details;
            
            MaxScore = details.Responses.Sum(r => r.MaxPoints);
            if (MaxScore > 0)
            {
                Percentage = ((details.TotalScore ?? 0) / MaxScore) * 100m;
            }
            
            IsPassed = Percentage >= PassingPercentageThreshold;

            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
