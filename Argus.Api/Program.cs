using System.Text;
using Argus.Data;
using Argus.Entities;
using Argus.Interfaces;
using Argus.Services;
using Argus.Services.Detection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

//
// =====================
// SERVICES (ALLES HIER)
// =====================
//

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB CONTEXT
builder.Services.AddDbContext<ArgusDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// IDENTITY
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ArgusDbContext>()
.AddDefaultTokenProviders();

// JWT AUTHENTICATION
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// NUGET HTTP CLIENT (used by NuGetEnricher)
builder.Services.AddHttpClient("NuGet", client =>
{
    client.BaseAddress = new Uri("https://api.nuget.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "Argus-Scanner/1.0");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// CUSTOM SERVICES
builder.Services.AddSingleton<HeuristicFilter>();
builder.Services.AddScoped<ISecretDetector, RegexDetector>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectUploadService, ProjectUploadService>();
builder.Services.AddScoped<IProjectFileScannerService, ProjectFileScannerService>();
builder.Services.AddSingleton<ICsprojParser, Argus.Services.Parsing.CsprojPackageParser>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<ISecretService, SecretService>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<INuGetEnricher, NuGetEnricher>();

var app = builder.Build();

//
// =====================
// HTTP PIPELINE
// =====================
//

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Argus API v1");
        options.RoutePrefix = string.Empty; // Swagger als root
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
