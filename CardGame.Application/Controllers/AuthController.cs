using System.Security.Claims;
using System.Text.RegularExpressions;
using CardGame.Application.Common.Interfaces;
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
    private readonly IUserRepository _userRepository;

    // Define password complexity regex (example: min 8 chars, 1 upper, 1 lower, 1 digit, 1 special)
    private static readonly Regex PasswordRegex = new Regex(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$",
        RegexOptions.Compiled);
    
    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <summary>
    /// Dummy login endpoint for testing. Assigns a new PlayerId based on username
    /// if the password meets basic complexity requirements. Creates an auth cookie.
    /// Uses IUserRepository to get/assign PlayerId.
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

        // Dummy Password Complexity Check
        if (string.IsNullOrEmpty(request.Password) || !PasswordRegex.IsMatch(request.Password))
        {
            return BadRequest(
                "Password does not meet complexity requirements (min 8 chars, 1 upper, 1 lower, 1 digit, 1 special character).");
        }

        // --- Use Repository to get or add Player ID ---
        Guid playerId = (await _userRepository.GetOrAddUserAsync(request.Username).ConfigureAwait(false)).PlayerId;

        // --- Create Authentication Cookie ---
        var claims = new List<Claim>
        {
            // Use NameIdentifier claim for the user's unique ID.
            // Using the PlayerId obtained from the repository.
            new Claim(ClaimTypes.NameIdentifier, playerId.ToString()),

            // Use Name claim for the username
            new Claim(ClaimTypes.Name, request.Username),

            // Add custom "PlayerId" claim specifically holding the game player Guid
            new Claim("PlayerId", playerId.ToString()),

            // Add other claims if needed (e.g., roles)
            // new Claim(ClaimTypes.Role, "Player"),
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme); // Specify the scheme

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            // Add other properties like ExpiresUtc if needed
        };

        // Sign the user in, creating the cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, // Specify the scheme
            new ClaimsPrincipal(claimsIdentity),
            authProperties).ConfigureAwait(false);

        // Return success response
        return Ok(new LoginResponseDto
        {
            Message = "Login successful (dummy).",
            Username = request.Username,
            PlayerId = playerId // Return the ID from the repository
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
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        return Ok(new { Message = "Logout successful." });
    }
}