using System.Text;
using Npgsql;
using System.Text.Json.Serialization;
using Healthcare.Api.Data;
using Healthcare.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is required.");

if (connection.StartsWith("postgresql://",
        StringComparison.OrdinalIgnoreCase) ||
    connection.StartsWith("postgres://",
        StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(connection);
    var userInfo = uri.UserInfo.Split(':', 2);

    var pgConnection = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1
            ? Uri.UnescapeDataString(userInfo[1])
            : "",
        Database = uri.AbsolutePath.TrimStart('/')
    };

    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseNpgsql(pgConnection.ConnectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlServer(connection));
}
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<SimplePdfService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<BkashPaymentService>();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddOpenApi();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();

var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:4200";
builder.Services.AddCors(o => o.AddPolicy("frontend", p =>
    p.WithOrigins(frontendUrl).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapGet("/", () => Results.Ok(new { success = true, message = "HealthFlow Healthcare API is running" }));
app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db, app.Configuration);
}

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    app.Run($"http://0.0.0.0:{port}");
else
    app.Run();
