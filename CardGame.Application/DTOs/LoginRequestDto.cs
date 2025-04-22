using System.ComponentModel.DataAnnotations;

namespace CardGame.Application.DTOs;

/// <summary>
/// Data required for the dummy login request.
/// </summary>
public class LoginRequestDto
{
    [Required]
    [MinLength(1)] // Basic username validation
    public string Username { get; set; } = string.Empty;

    [Required]
    // Password validation happens in the controller logic for this dummy endpoint
    public string Password { get; set; } = string.Empty;
}