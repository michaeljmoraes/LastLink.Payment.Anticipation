using LastLink.Payment.Anticipation.Api.Filters;
using LastLink.Payment.Anticipation.Infrastructure;
using LastLink.Payment.Anticipation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------
// CONTROLLERS + EXCEPTION FILTER
// Registers MVC controllers and attaches the global exception
// filter responsible for domain and system error normalization.
// -------------------------------------------------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});

// -------------------------------------------------------------
// SWAGGER / OPENAPI GENERATION
// Includes XML comments to enhance the generated documentation.
// Controllers, DTOs, and Models are automatically documented.
// -------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

// -------------------------------------------------------------
// CORS CONFIGURATION (ANGULAR FRONT-END ON PORT 4200)
// Enables controlled access for the SPA running during local dev.
// DefaultPolicy acts as fallback for environments requiring openness.
// -------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    // Optional fallback policy for broader development usage
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// -------------------------------------------------------------
// DATABASE CONFIGURATION (SQLite)
// Uses connection string from appsettings.json or defaults to a
// local SQLite file. DI registration is delegated to the
// Infrastructure layer via AddInfrastructure.
// -------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=anticipation.db";

builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// -------------------------------------------------------------
// MIDDLEWARE PIPELINE
// CORS MUST be applied before controller mapping.
// HTTPS redirection is enabled only in production to avoid
// interrupting local development flows.
// -------------------------------------------------------------
app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// -------------------------------------------------------------
// SWAGGER UI (DEV ONLY)
// Provides interactive API documentation for developers.
// -------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Maps attribute-routed controllers
app.MapControllers();

// -------------------------------------------------------------
// AUTOMATIC MIGRATION APPLICATION
// Ensures that the database schema is aligned with the current
// EF Core model at startup. Useful for local development and
// containerized execution.
// -------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnticipationDbContext>();
    db.Database.Migrate();
}

app.Run();
