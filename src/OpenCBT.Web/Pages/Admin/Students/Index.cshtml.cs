using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Students;

public class IndexModel : PageModel
{
    private readonly IStudentManagementService _studentService;

    public IndexModel(IStudentManagementService studentService)
    {
        _studentService = studentService;
    }

    public IEnumerable<StudentDto> Students { get; set; } = new List<StudentDto>();

    [BindProperty]
    public CreateStudentDto NewStudent { get; set; } = new();

    [BindProperty]
    public StudentDto EditStudent { get; set; } = new();

    [TempData]
    public string? TemporaryPassword { get; set; }

    [TempData]
    public string? ResetStudentName { get; set; }

    public async Task OnGetAsync()
    {
        Students = await _studentService.GetAllStudentsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            Students = await _studentService.GetAllStudentsAsync();
            return Page();
        }

        try
        {
            await _studentService.CreateStudentAsync(NewStudent);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Students = await _studentService.GetAllStudentsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        try
        {
            await _studentService.UpdateStudentAsync(EditStudent.Id, EditStudent);
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Students = await _studentService.GetAllStudentsAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _studentService.DeleteStudentAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(Guid id, string studentName)
    {
        try
        {
            var tempPass = await _studentService.ResetStudentPasswordAsync(id);
            TemporaryPassword = tempPass;
            ResetStudentName = studentName;
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Students = await _studentService.GetAllStudentsAsync();
            return Page();
        }
    }
}
