using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Enums;

namespace OpenCBT.Web.Pages.Admin.Exams;

public class CreateModel : PageModel
{
    private readonly IAdminExamService _adminExamService;
    private readonly IGradeService _gradeService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IAdminExamService adminExamService, IGradeService gradeService, ILogger<CreateModel> logger)
    {
        _adminExamService = adminExamService;
        _gradeService = gradeService;
        _logger = logger;
    }

    public SelectList Grades { get; set; } = null!;

    [BindProperty]
    public CreateExamDto Input { get; set; } = new()
    {
        // Round to nearest minute to prevent HTML5 datetime-local step validation errors
        StartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0, DateTimeKind.Utc),
        EndTime = new DateTime(DateTime.UtcNow.AddDays(7).Year, DateTime.UtcNow.AddDays(7).Month, DateTime.UtcNow.AddDays(7).Day, DateTime.UtcNow.AddDays(7).Hour, DateTime.UtcNow.AddDays(7).Minute, 0, DateTimeKind.Utc),
        DurationMinutes = 60,
        DisplayMode = DisplayMode.Wizard
    };

    public async Task OnGetAsync()
    {
        var grades = await _gradeService.GetAllAsync();
        Grades = new SelectList(grades, "Id", "Name");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine("========== ONPOSTASYNC EXECUTED ==========");
        Console.WriteLine($"INPUT TOKEN_REQUIRED: {Input.TokenRequired}");
        
        if (!ModelState.IsValid)
        {
            var errors = new List<string>();
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    errors.Add($"[{state.Key}]: {error.ErrorMessage}");
                }
            }
            Console.WriteLine("MODEL STATE INVALID: " + string.Join(" | ", errors));
            
            var grades = await _gradeService.GetAllAsync();
            Grades = new SelectList(grades, "Id", "Name");
            return Page();
        }

        await _adminExamService.CreateExamAsync(Input);
        return RedirectToPage("./Index");
    }
}
