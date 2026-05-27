using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

public class IndexModel : PageModel
{
    public string? Username { get; private set; }

    public IActionResult OnGet()
    {
        Username = HttpContext.Session.GetString("user");
        return Username is null ? RedirectToPage("/Login") : Page();
    }
}
