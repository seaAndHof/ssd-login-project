using System.ComponentModel.DataAnnotations;
using LoginPortal.Backend.Services;
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

    public AuthController(UserManager<IdentityUser> userManager, JwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupRequest request)
    {
        var user = new IdentityUser { UserName = request.Username, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.UserName!, roles);

        return Ok(new AuthResponse(token));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { errors = new[] { "Invalid username or password." } });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.UserName!, roles);

        return Ok(new AuthResponse(token));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            username = User.Identity?.Name,
        });
    }
}

public record SignupRequest(
    [Required] string Email,
    [Required] string Username,
    [Required] string Password);

public record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public record AuthResponse(string Token);
