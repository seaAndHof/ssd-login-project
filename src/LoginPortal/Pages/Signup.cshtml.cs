using LoginPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

public class SignupModel : PageModel
{
    private readonly AuthService _authService;

    public SignupModel(AuthService authService)
    {
        _authService = authService;
    }

    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";
    public List<string> Errors { get; set; } = [];

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != ConfirmPassword)
        {
            Errors.Add("Passwords do not match.");
            return Page();
        }

        var result = await _authService.SignupAsync(Email, Username, Password);

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

        Errors.AddRange(result.Errors);
        return Page();
    }
}
