using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        Response.Cookies.Delete("jwt");
        return RedirectToPage("/Login");
    }
}
