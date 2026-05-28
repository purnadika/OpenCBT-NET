using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenCBT.Application.Interfaces;

namespace OpenCBT.Web.Pages.Admin.Settings;

[Authorize(Policy = "AdminOrTeacher")]
public class IndexModel : PageModel
{
    private readonly ISystemSettingsService _settingsService;

    public IndexModel(ISystemSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [BindProperty]
    public string DefaultLanguage { get; set; } = "en-US";

    [BindProperty]
    public List<string> AvailableLanguages { get; set; } = new();

    public async Task OnGetAsync()
    {
        DefaultLanguage = await _settingsService.GetSettingAsync("DefaultLanguage") ?? "en-US";
        var availableLangs = await _settingsService.GetSettingAsync("AvailableLanguages") ?? "en-US,id-ID";
        AvailableLanguages = availableLangs.Split(',').Select(c => c.Trim()).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (AvailableLanguages == null || !AvailableLanguages.Any())
        {
            ModelState.AddModelError(string.Empty, "You must select at least one available language.");
            return Page();
        }

        // If the default language isn't in the available languages, add it or error out
        if (!AvailableLanguages.Contains(DefaultLanguage))
        {
            ModelState.AddModelError(string.Empty, "Default language must be one of the available languages.");
            return Page();
        }

        var availableLangsStr = string.Join(",", AvailableLanguages);

        await _settingsService.SetSettingAsync("DefaultLanguage", DefaultLanguage, "The default language for the site");
        await _settingsService.SetSettingAsync("AvailableLanguages", availableLangsStr, "Comma separated list of available languages");

        TempData["SuccessMessage"] = "Global localization settings updated successfully.";
        return RedirectToPage();
    }
}
