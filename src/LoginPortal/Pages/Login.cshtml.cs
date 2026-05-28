using LoginPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

public class LoginModel : PageModel
{
    private readonly AuthService _authService;

    public LoginModel(AuthService authService)
    {
        _authService = authService;
    }

    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _authService.LoginAsync(Username, Password);

        if (result.Succeeded)
        {
            Response.Cookies.Append("jwt", result.Token!, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
            return RedirectToPage("/Index");
        }

        ErrorMessage = result.Errors.FirstOrDefault() ?? "Invalid username or password.";
        return Page();
    }
}
