using LoginPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

public class LoginModel : PageModel
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;

    public LoginModel(AuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [BindProperty] public string Username { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public string ExternalLoginUrl =>
        $"{_configuration["Backend:BaseUrl"]!.TrimEnd('/')}/api/auth/external/login";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _authService.LoginAsync(Username, Password);

        if (result.Succeeded)
        {
            SetJwtCookie(result.Token!);
            return RedirectToPage("/Index");
        }

        if (result.MfaRequired)
        {
            TempData["MfaToken"] = result.MfaToken;
            return RedirectToPage("/Account/Mfa");
        }

        ErrorMessage = result.Errors.FirstOrDefault() ?? "Invalid username or password.";
        return Page();
    }

    private void SetJwtCookie(string token)
    {
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });
    }
}
