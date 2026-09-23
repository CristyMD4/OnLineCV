using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using OnlineCV.Api.Models;
using OnlineCV.Api.Services;

namespace OnlineCV.Api.Tests;

public sealed class AdminAuthServiceTests
{
    private static readonly JwtSettings Settings = new(
        "OnlineCV.Api",
        "OnlineCV.Admin",
        "test-signing-key-that-is-long-enough-for-hmac-sha256",
        TimeSpan.FromHours(8));

    private static AdminAuthService Create(params (string Key, string Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["Admin:Username"] = "admin",
            ["Admin:Password"] = "ChangeMe123!"
        };

        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new AdminAuthService(configuration, Settings);
    }

    [Fact]
    public void CorrectCredentialsReturnATokenForTheAdminRole()
    {
        var response = Create().Authenticate(new LoginRequest { Username = "admin", Password = "ChangeMe123!" });

        Assert.NotNull(response);
        Assert.Equal("admin", response.Username);
        Assert.NotEmpty(response.Token);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
        Assert.Equal(Settings.Issuer, token.Issuer);
        Assert.Contains(Settings.Audience, token.Audiences);
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
    }

    [Fact]
    public void TheTokenExpiresAfterTheConfiguredLifetime()
    {
        var response = Create().Authenticate(new LoginRequest { Username = "admin", Password = "ChangeMe123!" });

        Assert.NotNull(response);
        var expected = DateTime.UtcNow.Add(Settings.Lifetime);
        Assert.True((response.ExpiresAt - expected).Duration() < TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData("admin", "wrong-password")]
    [InlineData("wrong-user", "ChangeMe123!")]
    [InlineData("", "")]
    [InlineData("ADMIN", "ChangeMe123!")]
    public void WrongCredentialsAreRejected(string username, string password)
    {
        Assert.Null(Create().Authenticate(new LoginRequest { Username = username, Password = password }));
    }

    [Fact]
    public void EnvironmentStyleKeysWinOverAppSettings()
    {
        var service = Create(("ADMIN_USERNAME", "ops"), ("ADMIN_PASSWORD", "another-password"));

        Assert.NotNull(service.Authenticate(new LoginRequest { Username = "ops", Password = "another-password" }));
        Assert.Null(service.Authenticate(new LoginRequest { Username = "admin", Password = "ChangeMe123!" }));
    }

    [Fact]
    public void AMissingAdminPasswordFailsFastAtStartup()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Admin:Username"] = "admin" })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => new AdminAuthService(configuration, Settings));
        Assert.Contains("ADMIN_PASSWORD", exception.Message);
    }
}
