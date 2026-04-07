using Argus.Data;
using Argus.Interfaces;
using Argus.Services;
using Argus.Services.Detection;
using Microsoft.EntityFrameworkCore;

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

// CUSTOM SERVICES
builder.Services.AddSingleton<HeuristicFilter>();
builder.Services.AddScoped<ISecretDetector, RegexDetector>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectUploadService, ProjectUploadService>();
builder.Services.AddScoped<IProjectFileScannerService, ProjectFileScannerService>();
builder.Services.AddScoped<IScanService, ScanService>();
builder.Services.AddScoped<ISecretService, SecretService>();

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
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
