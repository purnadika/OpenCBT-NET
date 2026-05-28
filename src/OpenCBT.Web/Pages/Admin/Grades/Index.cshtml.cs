using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages.Admin.Grades;

public class IndexModel : PageModel
{
    private readonly IGradeService _gradeService;

    public IndexModel(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    public IEnumerable<Grade> Grades { get; set; } = new List<Grade>();

    [BindProperty]
    public Grade NewGrade { get; set; } = new();

    public async Task OnGetAsync()
    {
        Grades = await _gradeService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(NewGrade.Name))
        {
            await _gradeService.CreateAsync(new Grade { Name = NewGrade.Name });
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _gradeService.DeleteAsync(id);
        return RedirectToPage();
    }
}
