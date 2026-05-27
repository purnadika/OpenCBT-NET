using OpenCBT.Application.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages.Exams;

public class TakeModel : PageModel
{
    private readonly IExamService _examService;
    private readonly UserManager<ApplicationUser> _userManager;

    public TakeModel(IExamService examService, UserManager<ApplicationUser> userManager)
    {
        _examService = examService;
        _userManager = userManager;
    }

    public ExamDto Exam { get; set; } = null!;
    public ExamSessionDto? Session { get; set; }
    public bool RequiresTokenInput { get; set; }

    [BindProperty]
    public Guid SelectedOptionId { get; set; }
    
    [BindProperty]
    public Guid CurrentQuestionId { get; set; }

    [BindProperty]
    public string EssayAnswer { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id, string? token = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var exam = await _examService.GetExamByIdAsync(id);
        if (exam == null) return NotFound();

        Exam = exam;

        try
        {
            var request = new StartExamRequestDto { ExamId = id, UserId = user.Id, Token = token };
            Session = await _examService.StartExamAsync(request);
            if (Session.CompletedAt != null)
            {
                return RedirectToPage("./Result", new { sessionId = Session.Id });
            }
        }
        catch (ValidationException ex) when (ex.Message.Contains("token", StringComparison.OrdinalIgnoreCase))
        {
            RequiresTokenInput = true;
            if (token != null)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Index");
        }

        if (Exam.RandomizeQuestions && Session != null)
        {
            var random = new Random(Session.Id.GetHashCode());
            Exam.Questions = Exam.Questions.OrderBy(q => random.Next()).ToList();
            
            foreach (var q in Exam.Questions)
            {
                q.Options = q.Options.OrderBy(o => random.Next()).ToList();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostStartWithTokenAsync(Guid id, string token)
    {
        return await OnGetAsync(id, token);
    }

    public async Task<IActionResult> OnPostSubmitAnswerAsync(Guid id, Guid sessionId)
    {
        try
        {
            await _examService.SubmitAnswerAsync(sessionId, CurrentQuestionId, SelectedOptionId);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSubmitEssayAnswerAsync(Guid id, Guid sessionId)
    {
        try
        {
            await _examService.SubmitEssayAnswerAsync(sessionId, CurrentQuestionId, EssayAnswer ?? string.Empty);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostFinishExamAsync(Guid sessionId)
    {
        await _examService.CompleteExamAsync(sessionId);
        return RedirectToPage("./Result", new { sessionId });
    }
}
