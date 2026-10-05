using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Riesgos.Api.Domain;

namespace Riesgos.Api.Auth;

public static class AuthSetup
{
    public const string AdminPolicy = "Admin";

    public static IServiceCollection AddJwtAuth(this IServiceCollection services)
    {
        services.AddSingleton<TokenService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = TokenService.SigningKey(configuration),
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(AdminPolicy, policy => policy.RequireRole(nameof(Role.Admin)));
        return services;
    }

    /// <summary>Devuelve un token renovado en cada respuesta a un request autenticado (RNF-06).</summary>
    public static IApplicationBuilder UseSlidingSession(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tokens = context.RequestServices.GetRequiredService<TokenService>();
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[TokenService.RefreshHeader] = tokens.RefreshToken(context.User);
                    return Task.CompletedTask;
                });
            }
            await next();
        });
}
