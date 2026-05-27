using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Enums;

namespace OpenCBT.Web.Pages.Admin.Exams;

public class CreateModel : PageModel
{
    private readonly IAdminExamService _adminExamService;

    public CreateModel(IAdminExamService adminExamService)
    {
        _adminExamService = adminExamService;
    }

    [BindProperty]
    public CreateExamDto Input { get; set; } = new()
    {
        StartTime = DateTime.UtcNow,
        EndTime = DateTime.UtcNow.AddDays(7),
        DurationMinutes = 60,
        DisplayMode = DisplayMode.Wizard
    };

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _adminExamService.CreateExamAsync(Input);
        return RedirectToPage("./Index");
    }
}
