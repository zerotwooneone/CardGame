namespace CardGame.Application.DTOs;

/// <summary>
/// Response returned after a successful dummy login.
/// </summary>
public class LoginResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Guid PlayerId { get; set; } // The newly assigned Player ID
}