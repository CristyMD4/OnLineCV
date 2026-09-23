using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OnlineCV.Api.Data;
using OnlineCV.Api.Endpoints;
using OnlineCV.Api.Infrastructure;
using OnlineCV.Api.Models;
using OnlineCV.Api.Repositories;
using OnlineCV.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = ResolveJwtSettings(builder.Configuration);
var corsOrigins = ResolveCorsOrigins(builder.Configuration);
var connectionString = builder.Configuration.GetConnectionString("OnlineCv")
    ?? throw new InvalidOperationException(
        "Set ConnectionStrings:OnlineCv or ConnectionStrings__OnlineCv before starting the API.");

builder.Services.AddDbContextFactory<OnlineCvDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServer =>
        sqlServer.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ICvRepository, SqlCvRepository>();
builder.Services.AddSingleton<IContactRepository, SqlContactRepository>();
builder.Services.AddSingleton<CvContentProcessor>();
builder.Services.AddSingleton<CvService>();
builder.Services.AddSingleton<ContactService>();
builder.Services.AddSingleton<AdminAuthService>();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter the JWT returned by POST /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (corsOrigins.Contains("*")) policy.AllowAnyOrigin();
        else policy.WithOrigins(corsOrigins);

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://httpstatuses.com/429",
                title = "Too many requests",
                status = StatusCodes.Status429TooManyRequests,
                detail = "Wait before trying this operation again."
            },
            cancellationToken);
    };
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
    options.AddPolicy("contact", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var app = builder.Build();
await app.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "OnlineCV API v1");
    options.RoutePrefix = "swagger";
});
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapGet("/health", async (
    IDbContextFactory<OnlineCvDbContext> dbContextFactory,
    CancellationToken cancellationToken) =>
{
    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
    if (!await dbContext.Database.CanConnectAsync(cancellationToken))
    {
        return Results.Problem(
            title: "Database unavailable",
            detail: "The API cannot connect to SQL Server.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    return Results.Ok(new
    {
        status = "ok",
        service = "OnlineCV.Api",
        database = "connected",
        time = DateTimeOffset.UtcNow
    });
}).WithTags("Operations");

app.MapAuthEndpoints();
app.MapCvEndpoints();
app.MapContactEndpoints();

app.Run();

static JwtSettings ResolveJwtSettings(IConfiguration configuration)
{
    var issuer = configuration["JWT_ISSUER"] ?? configuration["Jwt:Issuer"] ?? "OnlineCV.Api";
    var audience = configuration["JWT_AUDIENCE"] ?? configuration["Jwt:Audience"] ?? "OnlineCV.Admin";
    var signingKey = configuration["JWT_KEY"] ?? configuration["Jwt:SigningKey"];

    if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
    {
        throw new InvalidOperationException("Set JWT_KEY or Jwt:SigningKey to at least 32 characters.");
    }

    var lifetimeValue = configuration["JWT_LIFETIME_HOURS"] ?? configuration["Jwt:LifetimeHours"];
    var lifetimeHours = int.TryParse(lifetimeValue, out var parsedLifetime)
        ? Math.Clamp(parsedLifetime, 1, 24)
        : 8;

    return new JwtSettings(issuer, audience, signingKey, TimeSpan.FromHours(lifetimeHours));
}

static string[] ResolveCorsOrigins(IConfiguration configuration)
{
    var environmentValue = configuration["CORS_ORIGIN"];
    if (!string.IsNullOrWhiteSpace(environmentValue)) return SplitOrigins(environmentValue);

    var configuredOrigins = configuration.GetSection("Cors:Origins")
        .GetChildren()
        .Select(section => section.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Cast<string>()
        .ToArray();

    return configuredOrigins.Length > 0
        ? configuredOrigins
        : SplitOrigins(configuration["Cors:Origin"] ?? "http://localhost:5173");
}

static string[] SplitOrigins(string value) =>
    value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
