namespace OnlineCV.Api.Models;

public sealed class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class LoginResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public string Username { get; set; } = "";
}

public sealed record JwtSettings(
    string Issuer,
    string Audience,
    string SigningKey,
    TimeSpan Lifetime);
