using LoginPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages.Account;

[Authorize]
public class MfaSetupModel : PageModel
{
    private readonly AuthService _authService;

    public MfaSetupModel(AuthService authService)
    {
        _authService = authService;
    }

    [BindProperty] public string Code { get; set; } = "";
    public MfaSetupInfo? Setup { get; private set; }
    public bool AlreadyEnabled { get; private set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadSetupAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt is null) return RedirectToPage("/Login");

        var result = await _authService.ConfirmMfaSetupAsync(jwt, Code);
        if (result.Succeeded)
        {
            AlreadyEnabled = true;
            return Page();
        }

        ErrorMessage = result.Errors.FirstOrDefault() ?? "Invalid code.";
        await LoadSetupAsync();
        return Page();
    }

    private async Task LoadSetupAsync()
    {
        var jwt = Request.Cookies["jwt"];
        if (jwt is null) return;

        Setup = await _authService.GetMfaSetupAsync(jwt);
        AlreadyEnabled = Setup?.Enabled ?? false;
    }
}
