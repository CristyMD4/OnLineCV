using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineCV.Api.Models;

namespace OnlineCV.Api.Services;

public sealed class AdminAuthService
{
    private readonly string _username;
    private readonly string _password;
    private readonly JwtSettings _jwtSettings;

    public AdminAuthService(IConfiguration configuration, JwtSettings jwtSettings)
    {
        _username = ResolveSetting(configuration, "ADMIN_USERNAME", "Admin:Username");
        _password = ResolveSetting(configuration, "ADMIN_PASSWORD", "Admin:Password");
        _jwtSettings = jwtSettings;
    }

    public LoginResponse? Authenticate(LoginRequest request)
    {
        var credentialsMatch = FixedTimeEquals(request.Username, _username)
            & FixedTimeEquals(request.Password, _password);

        if (!credentialsMatch)
        {
            return null;
        }

        var expiresAt = DateTime.UtcNow.Add(_jwtSettings.Lifetime);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, _username),
            new Claim(ClaimTypes.Name, _username),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            Username = _username
        };
    }

    private static bool FixedTimeEquals(string candidate, string expected)
    {
        var candidateHash = SHA256.HashData(Encoding.UTF8.GetBytes(candidate));
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(candidateHash, expectedHash);
    }

    private static string ResolveSetting(
        IConfiguration configuration,
        string environmentName,
        string configurationPath)
    {
        var value = configuration[environmentName] ?? configuration[configurationPath];
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"Set '{environmentName}' or '{configurationPath}' before starting the API.");
    }
}
