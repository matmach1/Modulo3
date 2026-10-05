using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Riesgos.Api.Domain;

namespace Riesgos.Api.Auth;

/// <summary>
/// Emite los JWT de sesión. RNF-06: cada token vence a las 24 h y se renueva
/// en cada request autenticado, de modo que la sesión expira por inactividad.
/// </summary>
public class TokenService(IConfiguration configuration)
{
    public const string RefreshHeader = "X-Session-Token";
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public static SymmetricSecurityKey SigningKey(IConfiguration configuration)
    {
        var key = configuration["JWT_KEY"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Falta la variable de entorno JWT_KEY (mínimo 32 caracteres).");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public string CreateToken(int userId, string email, Role role, DateTime? issuedAtUtc = null)
    {
        var issuedAt = issuedAtUtc ?? DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role.ToString()),
        };
        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: issuedAt,
            expires: issuedAt + Lifetime,
            signingCredentials: new SigningCredentials(SigningKey(configuration), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string RefreshToken(ClaimsPrincipal user) =>
        CreateToken(user.GetUserId(), user.FindFirstValue(ClaimTypes.Email)!, Enum.Parse<Role>(user.FindFirstValue(ClaimTypes.Role)!));
}

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
