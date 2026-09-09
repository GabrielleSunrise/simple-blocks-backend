using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SimpleBlocks.Application.Interfaces;
using SimpleBlocks.Application.Services;
using SimpleBlocks.Infrastructure;
using SimpleBlocks.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------------
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// CORS — в Development разрешаем любой источник (file://, любые локальные порты),
// в проде — только явный список из конфигурации (GitHub Pages / прод-домен).
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    policy.AllowAnyHeader()
          .AllowAnyMethod();

    if (builder.Environment.IsDevelopment())
    {
        policy.AllowAnyOrigin();
    }
    else
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? Array.Empty<string>();
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
    }
}));

// JWT configuration.
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = new JwtOptions
{
    Secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is required."),
    Issuer = jwtSection["Issuer"] ?? "SimpleBlocks",
    Audience = jwtSection["Audience"] ?? "SimpleBlocks.Client",
    ExpiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var expiry) ? expiry : 60,
    RefreshDays = int.TryParse(jwtSection["RefreshDays"], out var refreshDays) ? refreshDays : 30
};

if (!builder.Environment.IsDevelopment() &&
    jwtOptions.Secret.Contains("dev-only-change-me", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Jwt:Secret must be overridden (env Jwt__Secret) outside Development.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// Persistence + security services.
var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=simple-blocks.db";
builder.Services.AddInfrastructure(connectionString, jwtOptions);

// Application services.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBlockService, BlockService>();

var app = builder.Build();

// Ensure the SQLite database and schema exist on first run.
// (For schema evolution later, replace with EF Core migrations.)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SimpleBlocksDbContext>();
    db.Database.EnsureCreated();
}

// ---------------------------------------------------------------------------
// Pipeline
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // В Development редирект на HTTPS полезен (профиль https в launchSettings).
    // В Docker/на VPS TLS терминируется reverse-proxy, редирект не нужен.
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/healthz", () => Results.Ok("ok"));

app.Run();
