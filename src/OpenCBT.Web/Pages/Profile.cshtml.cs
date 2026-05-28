using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages;

[Authorize(Roles = "Student")]
public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentManagementService _studentService;

    public ProfileModel(UserManager<ApplicationUser> userManager, IStudentManagementService studentService)
    {
        _userManager = userManager;
        _studentService = studentService;
    }

    public ApplicationUser CurrentUser { get; set; } = null!;
    public ProfileUpdateRequestDto? PendingRequest { get; set; }

    [BindProperty]
    public SubmitProfileUpdateDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        CurrentUser = user;
        PendingRequest = await _studentService.GetPendingProfileUpdateAsync(user.Id);

        // Pre-fill the form with current values
        Input.RequestedFullName = user.FullName;
        Input.RequestedEmail = user.Email ?? string.Empty;
        Input.RequestedIdentifierNumber = user.IdentifierNumber;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return await OnGetAsync();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await _studentService.SubmitProfileUpdateAsync(user.Id, Input);

        TempData["SuccessMessage"] = "Your profile update request has been submitted for review.";
        return RedirectToPage();
    }
}
