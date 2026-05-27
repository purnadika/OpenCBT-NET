using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Exams;

public class IndexModel : PageModel
{
    private readonly IAdminExamService _adminExamService;

    public IndexModel(IAdminExamService adminExamService)
    {
        _adminExamService = adminExamService;
    }

    public IEnumerable<ExamDto> Exams { get; set; } = new List<ExamDto>();

    public async Task OnGetAsync()
    {
        Exams = await _adminExamService.GetAllExamsAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _adminExamService.DeleteExamAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGenerateTokenAsync(Guid id)
    {
        await _adminExamService.GenerateTokenAsync(id);
        return RedirectToPage();
    }
}
