using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Questions;

public class IndexModel : PageModel
{
    private readonly IAdminExamService _adminExamService;
    private readonly IFileStorageService _fileStorageService;

    public IndexModel(IAdminExamService adminExamService, IFileStorageService fileStorageService)
    {
        _adminExamService = adminExamService;
        _fileStorageService = fileStorageService;
    }

    public Guid ExamId { get; set; }
    public ExamDto Exam { get; set; } = null!;
    public IEnumerable<QuestionDto> Questions { get; set; } = new List<QuestionDto>();

    [BindProperty]
    public QuestionDto NewQuestion { get; set; } = new();

    [BindProperty]
    public IFormFile? QuestionImage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid examId)
    {
        ExamId = examId;
        var exam = await _adminExamService.GetExamByIdAsync(examId);
        if (exam == null) return NotFound();

        Exam = exam;
        Questions = await _adminExamService.GetQuestionsByExamIdAsync(examId);
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(Guid examId)
    {
        if (!ModelState.IsValid)
        {
            var exam = await _adminExamService.GetExamByIdAsync(examId);
            if (exam == null) return NotFound();
            Exam = exam;
            Questions = await _adminExamService.GetQuestionsByExamIdAsync(examId);
            return Page();
        }

        try
        {
            if (QuestionImage != null && QuestionImage.Length > 0)
            {
                var relativePath = await _fileStorageService.SaveFileAsync(QuestionImage, "questions");
                NewQuestion.ImageUrl = relativePath;
            }

            await _adminExamService.AddQuestionAsync(examId, NewQuestion);
            return RedirectToPage(new { examId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var exam = await _adminExamService.GetExamByIdAsync(examId);
            if (exam == null) return NotFound();
            Exam = exam;
            Questions = await _adminExamService.GetQuestionsByExamIdAsync(examId);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid examId, Guid questionId)
    {
        await _adminExamService.DeleteQuestionAsync(questionId);
        return RedirectToPage(new { examId });
    }
}
