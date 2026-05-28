using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

public class IndexModel : PageModel
{
    public string? Username { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToPage("/Login");

        Username = User.Identity.Name;
        return Page();
    }
}
