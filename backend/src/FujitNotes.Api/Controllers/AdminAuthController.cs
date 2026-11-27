
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FujitNotes.Api.Controllers;

[ApiController]
[Route("api/admin/auth")]
public sealed class AdminAuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<string> _passwordHasher = new();

    public AdminAuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // POST /api/admin/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("admin-login")]
    public async Task<IActionResult> Login(
        [FromBody] AdminLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "ユーザー名とパスワードを入力してください。"
            });
        }

        // 秘密情報はソースコードには保存しない
        var expectedUsername =
            _configuration["AdminAuth:Username"];

        var storedPasswordHash =
            _configuration["AdminAuth:PasswordHash"];

        if (string.IsNullOrWhiteSpace(expectedUsername)
            || string.IsNullOrWhiteSpace(storedPasswordHash))
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = "管理者アカウントが未設定です。"
                });
        }

        PasswordVerificationResult verification;

        try
        {
            verification =
                _passwordHasher.VerifyHashedPassword(
                    expectedUsername,
                    storedPasswordHash,
                    request.Password);
        }
        catch (FormatException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable);
        }

        var usernameMatches = string.Equals(
            request.Username,
            expectedUsername,
            StringComparison.Ordinal);

        if (!usernameMatches
            || verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "ユーザー名またはパスワードが違います。"
            });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "fujit-admin"),
            new(ClaimTypes.Name, expectedUsername),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return Ok(new
        {
            authenticated = true,
            username = expectedUsername
        });
    }

    // GET /api/admin/auth/me
    [HttpGet("me")]
    [Authorize(Roles = "Admin")]
    public IActionResult Me()
    {
        return Ok(new
        {
            authenticated = true,
            username = User.Identity?.Name
        });
    }

    // POST /api/admin/auth/logout
    [HttpPost("logout")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }
}

public sealed record AdminLoginRequest(
    string Username,
    string Password);
