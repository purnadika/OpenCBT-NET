using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages.Exams;

public class IndexModel : PageModel
{
    private readonly IExamService _examService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IExamService examService, UserManager<ApplicationUser> userManager)
    {
        _examService = examService;
        _userManager = userManager;
    }

    public IEnumerable<ExamDto> Exams { get; set; } = new List<ExamDto>();
    public IEnumerable<ExamSessionDto> ExamHistory { get; set; } = new List<ExamSessionDto>();

    // Statistics
    public decimal AverageScore { get; set; }
    public int CompletedCount { get; set; }
    public int ActiveCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login", new { returnUrl = "/Exams" });

        Exams = await _examService.GetActiveExamsAsync(user.Id);
        ActiveCount = Exams.Count();

        var history = await _examService.GetStudentExamHistoryAsync(user.Id);
        ExamHistory = history;
        CompletedCount = history.Count();

        if (CompletedCount > 0)
        {
            AverageScore = history.Average(h => h.TotalScore ?? 0);
        }

        return Page();
    }
}
