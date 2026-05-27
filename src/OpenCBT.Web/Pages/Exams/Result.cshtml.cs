using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages.Exams;

public class ResultModel : PageModel
{
    private readonly IExamService _examService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResultModel(IExamService examService, UserManager<ApplicationUser> userManager)
    {
        _examService = examService;
        _userManager = userManager;
    }

    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid sessionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var review = await _examService.GetStudentSessionReviewAsync(sessionId, user.Id);

        if (review == null)
            return NotFound();

        Score = review.TotalScore ?? 0;
        MaxScore = review.Responses.Sum(r => r.MaxPoints);

        return Page();
    }
}
