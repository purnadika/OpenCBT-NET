using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages.Admin.ProfileApprovals;

[Authorize(Roles = "Admin,Teacher")]
public class IndexModel : PageModel
{
    private readonly IStudentManagementService _studentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IStudentManagementService studentService, UserManager<ApplicationUser> userManager)
    {
        _studentService = studentService;
        _userManager = userManager;
    }

    public IEnumerable<ProfileUpdateRequestDto> PendingRequests { get; set; } = new List<ProfileUpdateRequestDto>();

    public async Task OnGetAsync()
    {
        PendingRequests = await _studentService.GetAllPendingProfileUpdatesAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var admin = await _userManager.GetUserAsync(User);
        if (admin == null) return NotFound();

        try
        {
            await _studentService.ApproveProfileUpdateAsync(id, admin.Id);
            TempData["SuccessMessage"] = "Profile update request approved and applied successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to approve request: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        var admin = await _userManager.GetUserAsync(User);
        if (admin == null) return NotFound();

        try
        {
            await _studentService.RejectProfileUpdateAsync(id, admin.Id);
            TempData["SuccessMessage"] = "Profile update request has been rejected.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to reject request: {ex.Message}";
        }

        return RedirectToPage();
    }
}
