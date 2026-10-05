using Riesgos.Api.Domain;
using Riesgos.Api.Endpoints;

namespace Riesgos.Api.Data;

/// <summary>RF-30: crea el administrador inicial con credenciales de ADMIN_EMAIL y ADMIN_PASSWORD.</summary>
public static class AdminSeeder
{
    public static void Seed(AppDbContext db, IConfiguration configuration, ILogger logger)
    {
        var email = configuration["ADMIN_EMAIL"];
        var password = configuration["ADMIN_PASSWORD"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("ADMIN_EMAIL o ADMIN_PASSWORD no están definidas: no se crea el administrador inicial.");
            return;
        }

        email = AuthEndpoints.NormalizeEmail(email);
        if (db.Users.Any(u => u.Email == email))
            return;

        db.Users.Add(new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = Role.Admin,
        });
        db.SaveChanges();
    }
}
