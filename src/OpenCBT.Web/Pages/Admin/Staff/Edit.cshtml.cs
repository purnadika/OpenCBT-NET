using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Staff;

[Authorize(Policy = "AdminOrTeacher")]
public class EditModel : PageModel
{
    private readonly IStaffManagementService _staffService;

    public EditModel(IStaffManagementService staffService)
    {
        _staffService = staffService;
    }

    [BindProperty]
    public UpdateStaffDto Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var staffList = await _staffService.GetAllStaffAsync();
        var staff = staffList.FirstOrDefault(s => s.Id == Id);

        if (staff == null)
        {
            return NotFound();
        }

        Input = new UpdateStaffDto
        {
            FullName = staff.FullName,
            Email = staff.Email,
            IdentifierNumber = staff.IdentifierNumber,
            Role = staff.Role,
            IsActive = staff.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _staffService.UpdateStaffAsync(Id, Input);
            TempData["SuccessMessage"] = $"Staff member {Input.FullName} updated successfully.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostResetPasswordAsync()
    {
        try
        {
            var newPassword = await _staffService.ResetStaffPasswordAsync(Id);
            TempData["SuccessMessage"] = $"Password has been reset successfully. The new temporary password is: {newPassword}";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }
}
