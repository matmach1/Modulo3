using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Riesgos.Api.Auth;
using Riesgos.Api.Data;
using Riesgos.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>((services, options) =>
    options.UseSqlite(services.GetRequiredService<IConfiguration>().GetConnectionString("Default")));
builder.Services.AddJwtAuth();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Falla al arrancar si falta JWT_KEY, en lugar de fallar en el primer login.
TokenService.SigningKey(app.Configuration);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    AdminSeeder.Seed(db, app.Configuration, app.Logger);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseSlidingSession();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapAuthEndpoints();
app.MapReportEndpoints();

app.Run();

public partial class Program { }
