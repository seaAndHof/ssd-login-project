using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LoginPortal.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/auth/mfa")]
public class MfaController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public MfaController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("setup")]
    public async Task<IActionResult> Setup()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var otpauth = BuildOtpAuthUri("LoginPortal", user.UserName!, key!);
        return Ok(new { secret = key, otpauthUri = otpauth, enabled = user.TwoFactorEnabled });
    }

    [HttpPost("setup")]
    public async Task<IActionResult> ConfirmSetup(MfaSetupRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var valid = await _userManager.VerifyTwoFactorTokenAsync(
            user, TokenOptions.DefaultAuthenticatorProvider, request.Code);
        if (!valid)
            return BadRequest(new { errors = new[] { "Invalid authenticator code." } });

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        return Ok(new { enabled = true });
    }

    private static string BuildOtpAuthUri(string issuer, string account, string secret)
    {
        var label = Uri.EscapeDataString($"{issuer}:{account}");
        var sb = new StringBuilder("otpauth://totp/");
        sb.Append(label);
        sb.Append("?secret=").Append(secret);
        sb.Append("&issuer=").Append(Uri.EscapeDataString(issuer));
        sb.Append("&digits=6&period=30");
        return sb.ToString();
    }
}

public record MfaSetupRequest([Required] string Code);
