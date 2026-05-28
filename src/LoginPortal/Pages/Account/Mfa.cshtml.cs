using LoginPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages.Account;

public class MfaModel : PageModel
{
    private readonly AuthService _authService;

    public MfaModel(AuthService authService)
    {
        _authService = authService;
    }

    [BindProperty] public string Code { get; set; } = "";
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (TempData.Peek("MfaToken") is not string)
            return RedirectToPage("/Login");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TempData["MfaToken"] is not string mfaToken)
            return RedirectToPage("/Login");

        var result = await _authService.VerifyMfaAsync(mfaToken, Code);

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

        TempData["MfaToken"] = mfaToken;
        ErrorMessage = result.Errors.FirstOrDefault() ?? "Invalid code.";
        return Page();
    }
}
