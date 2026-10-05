using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Riesgos.Api.Auth;
using Riesgos.Api.Data;
using Riesgos.Api.Domain;

namespace Riesgos.Api.Endpoints;

public record RegisterRequest(string? Email, string? Password);
public record LoginRequest(string? Email, string? Password);

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

        // RF-12
        group.MapPost("/login", async (LoginRequest request, AppDbContext db, TokenService tokens) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.Unauthorized();

            var email = NormalizeEmail(request.Email);
            var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Results.Unauthorized();

            var token = tokens.CreateToken(user.Id, user.Email, user.Role);
            return Results.Ok(new { token, user.Email, Role = user.Role.ToString() });
        });

        group.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            Id = user.GetUserId(),
            Email = user.FindFirstValue(ClaimTypes.Email),
            Role = user.FindFirstValue(ClaimTypes.Role),
        })).RequireAuthorization();
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
