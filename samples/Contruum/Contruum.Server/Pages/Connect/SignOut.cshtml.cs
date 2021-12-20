using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Contruum.Server.Pages.Connect;

public class SignOutModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("Index");

    public IActionResult OnPost(string? returnUrl = null)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/connect/signin"
        };

        return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
