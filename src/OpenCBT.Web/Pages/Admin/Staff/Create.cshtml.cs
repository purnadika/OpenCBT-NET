using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Staff;

[Authorize(Policy = "AdminOrTeacher")]
public class CreateModel : PageModel
{
    private readonly IStaffManagementService _staffService;

    public CreateModel(IStaffManagementService staffService)
    {
        _staffService = staffService;
    }

    [BindProperty]
    public CreateStaffDto Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var newStaff = await _staffService.CreateStaffAsync(Input);
            
            if (string.IsNullOrEmpty(Input.Password))
            {
                TempData["SuccessMessage"] = $"Staff member {newStaff.FullName} created successfully. Auto-generated password: {newStaff.RawPassword}";
            }
            else
            {
                TempData["SuccessMessage"] = $"Staff member {newStaff.FullName} created successfully.";
            }

            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
