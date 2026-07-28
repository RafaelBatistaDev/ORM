using BlogManager.Data;
using BlogManager.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// Services
// ──────────────────────────────────────────────

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BlogManager API",
        Description = "A professional blog administration API built with ASP.NET Core and Entity Framework Core.",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Rafael Batista",
            Email = "recifecrypto@gmail.com"
        }
    });
});

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (corsOrigins is { Length: > 0 })
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowedOrigins", policy =>
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });
}
else
{
    // Fallback: allow all in development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowedOrigins", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });
}

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// ──────────────────────────────────────────────
#region Middleware Pipeline
// ──────────────────────────────────────────────

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Development tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogManager API v1");
        c.RoutePrefix = "swagger";
    });
}

// Security & routing
app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// ──────────────────────────────────────────────
// Auto-migrate database (development convenience)
// ──────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

#endregion

app.Run();
