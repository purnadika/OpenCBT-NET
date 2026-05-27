using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace OpenCBT.Web.Controllers;

[Route("[controller]/[action]")]
public class CultureController : Controller
{
    [HttpPost]
    public IActionResult SetCulture(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true }
        );

        return LocalRedirect(returnUrl);
    }
}
