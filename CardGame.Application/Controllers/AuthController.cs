using System.Security.Claims;
using System.Text.RegularExpressions;
using CardGame.Application.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardGame.Application.Controllers;

[ApiController]
[Route("api/[controller]")] // Route: /api/Auth
public class AuthController : ControllerBase
{
    // Define password complexity regex (example: min 8 chars, 1 upper, 1 lower, 1 digit, 1 special)
    private static readonly Regex PasswordRegex = new Regex(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Dummy login endpoint for testing. Assigns a new PlayerId based on username
    /// if the password meets basic complexity requirements. Creates an auth cookie.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>Login response with assigned PlayerId or BadRequest.</returns>
    [HttpPost("login")]
    [AllowAnonymous] // Allow access to login even if not authenticated
    [ProducesResponseType(typeof(LoginResponseDto), 200)] // OK
    [ProducesResponseType(400)] // Bad Request
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        // Basic model validation
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        //for testing "playerId": "b2e28463-4b18-f5cb-8ecc-f10ca7a6563c","name": "Alice",
        // Dummy Password Complexity Check
        if (string.IsNullOrEmpty(request.Password) || !PasswordRegex.IsMatch(request.Password))
        {
            return BadRequest("Password does not meet complexity requirements (min 8 chars, 1 upper, 1 lower, 1 digit, 1 special character).");
        }

        // --- Dummy Login Logic ---
        // In a real app: Verify username/password hash against a user store.
        // Here: Generate a NEW PlayerId just based on the username provided.
        // This is NOT secure and only for testing the authentication flow.
        // Using a consistent Guid generation based on username for predictability in testing:
        var newPlayerId = GenerateDeterministicGuid(request.Username);

        // --- Create Authentication Cookie ---
        var claims = new List<Claim>
        {
            // Use NameIdentifier claim for the user's unique ID (our PlayerId)
            new Claim(ClaimTypes.NameIdentifier, newPlayerId.ToString()),
            // Use Name claim for the username
            new Claim(ClaimTypes.Name, request.Username),
            // Add other claims if needed (e.g., roles)
            // new Claim(ClaimTypes.Role, "Player"),
            new Claim("PlayerId", newPlayerId.ToString()),
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme); // Specify the scheme

        var authProperties = new AuthenticationProperties
        {
            // AllowRefresh = <bool>,
            // Refreshing the authentication session should be allowed.

            // ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60),
            // The time at which the authentication ticket expires. A
            // value set here overrides the ExpireTimeSpan option of
            // CookieAuthenticationOptions set with AddCookie.

            IsPersistent = true,
            // Whether the authentication session is persisted across
            // multiple requests. When used with cookies, controls
            // whether the cookie's lifetime is absolute (matching the
            // lifetime of the authentication ticket) or session-based.

            // IssuedUtc = <DateTimeOffset>,
            // The time at which the authentication ticket was issued.

            // RedirectUri = <string>
            // The full path or absolute URI to be used as an http
            // redirect response value.
        };

        // Sign the user in, creating the cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, // Specify the scheme
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // Return success response
        return Ok(new LoginResponseDto
        {
            Message = "Login successful.",
            Username = request.Username,
            PlayerId = newPlayerId
        });
    }

    /// <summary>
    /// Dummy logout endpoint.
    /// </summary>
    [HttpPost("logout")]
    // [Authorize] // Should require authorization to log out
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout()
    {
        // Sign the user out, removing the cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { Message = "Logout successful." });
    }


    // Helper to generate a predictable Guid from a string (for testing consistency)
    private static Guid GenerateDeterministicGuid(string input)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
    }
}