using OnlineCV.Api.Models;
using OnlineCV.Api.Services;

namespace OnlineCV.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", (LoginRequest request, AdminAuthService authService) =>
            {
                var response = authService.Authenticate(request);
                return response is null ? Results.Unauthorized() : Results.Ok(response);
            })
            .RequireRateLimiting("login")
            .WithTags("Authentication")
            .WithSummary("Create an administrator JWT");

        return endpoints;
    }
}
