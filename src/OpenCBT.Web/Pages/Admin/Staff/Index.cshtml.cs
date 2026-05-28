using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.DTOs;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Staff;

[Authorize(Policy = "AdminOrTeacher")]
public class IndexModel : PageModel
{
    private readonly IStaffManagementService _staffService;

    public IndexModel(IStaffManagementService staffService)
    {
        _staffService = staffService;
    }

    public IEnumerable<StaffDto> StaffList { get; set; } = new List<StaffDto>();

    public async Task OnGetAsync()
    {
        StaffList = await _staffService.GetAllStaffAsync();
    }
}
