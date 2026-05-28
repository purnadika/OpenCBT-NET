using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using Microsoft.Extensions.Localization;
using OpenCBT.Application.Exceptions;

namespace OpenCBT.Web.Pages.Admin.Students;

public class IndexModel : PageModel
{
    private readonly IStudentManagementService _studentService;
    private readonly IGradeService _gradeService;
    private readonly IClassRoomService _classRoomService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(IStudentManagementService studentService, IGradeService gradeService, IClassRoomService classRoomService, IStringLocalizer<SharedResource> localizer)
    {
        _studentService = studentService;
        _gradeService = gradeService;
        _classRoomService = classRoomService;
        _localizer = localizer;
    }

    public IEnumerable<StudentDto> Students { get; set; } = new List<StudentDto>();
    public IEnumerable<OpenCBT.Domain.Entities.Grade> Grades { get; set; } = new List<OpenCBT.Domain.Entities.Grade>();
    public IEnumerable<OpenCBT.Domain.Entities.ClassRoom> ClassRooms { get; set; } = new List<OpenCBT.Domain.Entities.ClassRoom>();

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
        Grades = await _gradeService.GetAllAsync();
        ClassRooms = await _classRoomService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            Students = await _studentService.GetAllStudentsAsync();
            Grades = await _gradeService.GetAllAsync();
            ClassRooms = await _classRoomService.GetAllAsync();
            return Page();
        }

        try
        {
            await _studentService.CreateStudentAsync(NewStudent);
            return RedirectToPage();
        }
        catch (ValidationException ex)
        {
            var localizedMessage = _localizer[ex.Message, ex.Args];
            ModelState.AddModelError(string.Empty, localizedMessage);
            Students = await _studentService.GetAllStudentsAsync();
            Grades = await _gradeService.GetAllAsync();
            ClassRooms = await _classRoomService.GetAllAsync();
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Students = await _studentService.GetAllStudentsAsync();
            Grades = await _gradeService.GetAllAsync();
            ClassRooms = await _classRoomService.GetAllAsync();
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
        catch (ValidationException ex)
        {
            var localizedMessage = _localizer[ex.Message, ex.Args];
            ModelState.AddModelError(string.Empty, localizedMessage);
            Students = await _studentService.GetAllStudentsAsync();
            Grades = await _gradeService.GetAllAsync();
            ClassRooms = await _classRoomService.GetAllAsync();
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Students = await _studentService.GetAllStudentsAsync();
            Grades = await _gradeService.GetAllAsync();
            ClassRooms = await _classRoomService.GetAllAsync();
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
        catch (ValidationException ex)
        {
            var localizedMessage = _localizer[ex.Message, ex.Args];
            ModelState.AddModelError(string.Empty, localizedMessage);
            Students = await _studentService.GetAllStudentsAsync();
            Grades = await _gradeService.GetAllAsync();
            ClassRooms = await _classRoomService.GetAllAsync();
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Students = await _studentService.GetAllStudentsAsync();
            Grades = await _gradeService.GetAllAsync();
            ClassRooms = await _classRoomService.GetAllAsync();
            return Page();
        }
    }
}
