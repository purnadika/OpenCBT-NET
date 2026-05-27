using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages.Exams;

public class ReviewModel : PageModel
{
    private readonly IExamService _examService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewModel(IExamService examService, UserManager<ApplicationUser> userManager)
    {
        _examService = examService;
        _userManager = userManager;
    }

    public ExamSessionDetailsDto SessionDetails { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid sessionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        try
        {
            var details = await _examService.GetStudentSessionReviewAsync(sessionId, user.Id);
            if (details == null) return NotFound();

            SessionDetails = details;
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
