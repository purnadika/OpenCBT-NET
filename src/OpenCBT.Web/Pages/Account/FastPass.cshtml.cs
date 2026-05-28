using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenCBT.Domain.Entities;
using OpenCBT.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace OpenCBT.Web.Pages.Account;

public class FastPassModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public FastPassModel(SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "NISN is required.")]
        [Display(Name = "NISN (Nomor Induk Siswa Nasional)")]
        public string IdentifierNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Exam Token is required.")]
        [Display(Name = "Exam Token")]
        public string ExamToken { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var student = await _signInManager.UserManager.Users
            .FirstOrDefaultAsync(u => u.IdentifierNumber == Input.IdentifierNumber && u.IsActive);

        if (student == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid NISN or account is inactive.");
            return Page();
        }

        var isStudent = await _signInManager.UserManager.IsInRoleAsync(student, "Student");
        if (!isStudent)
        {
            ModelState.AddModelError(string.Empty, "Only students can use Fast Pass.");
            return Page();
        }

        var exam = await _context.Set<Exam>().FirstOrDefaultAsync(e => e.CurrentToken == Input.ExamToken && e.IsActive);
        if (exam == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid or expired Exam Token.");
            return Page();
        }

        await _signInManager.SignInAsync(student, isPersistent: false);

        // Redirect directly to the exam execution page
        return RedirectToPage("/Exams/Take", new { id = exam.Id });
    }
}
