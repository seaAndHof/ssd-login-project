using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

public class LoginModel : PageModel
{
    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";

    public void OnGet() { }

    public IActionResult OnPost()
    {
        HttpContext.Session.SetString("user", Username);
        return RedirectToPage("/Index");
    }
}
