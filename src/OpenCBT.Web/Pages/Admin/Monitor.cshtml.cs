using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin;

public class MonitorModel : PageModel
{
    private readonly IAdminExamService _adminExamService;

    public MonitorModel(IAdminExamService adminExamService)
    {
        _adminExamService = adminExamService;
    }

    public ExamDto Exam { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid examId)
    {
        var exam = await _adminExamService.GetExamByIdAsync(examId);
        if (exam == null) return NotFound();

        Exam = exam;
        return Page();
    }
}
