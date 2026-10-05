using Microsoft.EntityFrameworkCore;
using Riesgos.Api.Data;
using Riesgos.Api.Domain;

namespace Riesgos.Api.Endpoints;

public record RegisterRequest(string? Email, string? Password);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        // RF-14, RF-29: todo registro crea un usuario común.
        group.MapPost("/register", async (RegisterRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new { error = "El email y la contraseña son obligatorios." });

            var email = NormalizeEmail(request.Email);
            if (await db.Users.AnyAsync(u => u.Email == email))
                return Results.Conflict(new { error = "El email ya está registrado." });

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Role.Comun,
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email, Role = user.Role.ToString() });
        });
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
