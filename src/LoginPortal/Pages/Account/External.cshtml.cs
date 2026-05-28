using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages.Account;

public class ExternalModel : PageModel
{
    public IActionResult OnGet(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Login");

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return RedirectToPage("/Index");
    }
}
