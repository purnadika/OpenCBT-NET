using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.Interfaces;
using OpenCBT.Domain.Entities;

namespace OpenCBT.Web.Pages.Admin.Classes;

public class IndexModel : PageModel
{
    private readonly IClassRoomService _classRoomService;

    public IndexModel(IClassRoomService classRoomService)
    {
        _classRoomService = classRoomService;
    }

    public IEnumerable<ClassRoom> ClassRooms { get; set; } = new List<ClassRoom>();

    [BindProperty]
    public ClassRoom NewClassRoom { get; set; } = new();

    public async Task OnGetAsync()
    {
        ClassRooms = await _classRoomService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(NewClassRoom.Name))
        {
            await _classRoomService.CreateAsync(new ClassRoom { Name = NewClassRoom.Name });
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _classRoomService.DeleteAsync(id);
        return RedirectToPage();
    }
}
