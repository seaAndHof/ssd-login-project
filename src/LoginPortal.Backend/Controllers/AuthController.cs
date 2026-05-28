using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LoginPortal.Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LoginPortal.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        JwtService jwtService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupRequest request)
    {
        var user = new IdentityUser { UserName = request.Username, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, "User");
        return Ok(await IssueTokenAsync(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { errors = new[] { "Invalid username or password." } });

        if (user.TwoFactorEnabled)
        {
            var pending = _jwtService.GenerateMfaPendingToken(user.Id);
            return Ok(new LoginResponse(null, true, pending));
        }

        return Ok(await IssueTokenAsync(user));
    }

    [HttpPost("mfa/verify")]
    public async Task<IActionResult> VerifyMfa(MfaVerifyRequest request)
    {
        var userId = _jwtService.ValidateMfaPendingToken(request.MfaToken);
        if (userId is null)
            return Unauthorized(new { errors = new[] { "MFA challenge expired or invalid." } });

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Unauthorized(new { errors = new[] { "User not found." } });

        var valid = await _userManager.VerifyTwoFactorTokenAsync(
            user, TokenOptions.DefaultAuthenticatorProvider, request.Code);
        if (!valid)
            return Unauthorized(new { errors = new[] { "Invalid authenticator code." } });

        return Ok(await IssueTokenAsync(user));
    }

    [HttpGet("external/login")]
    public IActionResult ExternalLogin()
    {
        var redirectUri = Url.Action(nameof(ExternalCallback))!;
        var properties = new AuthenticationProperties { RedirectUri = redirectUri };
        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("external/callback")]
    public async Task<IActionResult> ExternalCallback()
    {
        var auth = await HttpContext.AuthenticateAsync("ExternalCookies");
        if (!auth.Succeeded || auth.Principal is null)
            return Unauthorized(new { errors = new[] { "External sign-in failed." } });

        var subject = auth.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? auth.Principal.FindFirst("sub")?.Value;
        var email = auth.Principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? auth.Principal.FindFirst("email")?.Value;
        var username = auth.Principal.FindFirst("preferred_username")?.Value
                       ?? auth.Principal.Identity?.Name
                       ?? email
                       ?? subject;

        if (subject is null || username is null)
            return Unauthorized(new { errors = new[] { "External token missing required claims." } });

        var loginProvider = OpenIdConnectDefaults.AuthenticationScheme;
        var user = await _userManager.FindByLoginAsync(loginProvider, subject);
        if (user is null)
        {
            user = new IdentityUser { UserName = username, Email = email };
            var create = await _userManager.CreateAsync(user);
            if (!create.Succeeded)
                return BadRequest(new { errors = create.Errors.Select(e => e.Description) });

            await _userManager.AddLoginAsync(user, new UserLoginInfo(loginProvider, subject, "OIDC"));
            await _userManager.AddToRoleAsync(user, "User");
        }

        await HttpContext.SignOutAsync("ExternalCookies");

        var token = (await IssueTokenAsync(user)).Token!;
        var frontend = _configuration["Frontend:BaseUrl"]!.TrimEnd('/');
        return Redirect($"{frontend}/Account/External?token={Uri.EscapeDataString(token)}");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            username = User.Identity?.Name,
            roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value),
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = _userManager.Users.ToList();
        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new { user.Id, user.UserName, user.Email, Roles = roles });
        }

        return Ok(result);
    }

    private async Task<LoginResponse> IssueTokenAsync(IdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.UserName!, roles);
        return new LoginResponse(token, false, null);
    }
}

public record SignupRequest(
    [Required] string Email,
    [Required] string Username,
    [Required] string Password);

public record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public record MfaVerifyRequest(
    [Required] string MfaToken,
    [Required] string Code);

public record LoginResponse(string? Token, bool MfaRequired, string? MfaToken);
