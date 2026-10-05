using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Riesgos.Api.Auth;
using Riesgos.Api.Domain;

namespace Riesgos.Tests;

public class LoginTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // AC-13
    [Fact]
    public async Task Usuario_registrado_puede_autenticarse()
    {
        var email = TestAuth.NewEmail();
        await _client.Register(email);

        _client.UseToken(await _client.Login(email));
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_con_contraseña_incorrecta_responde_401()
    {
        var email = TestAuth.NewEmail();
        await _client.Register(email);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "incorrecta" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Sin_token_responde_401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_autenticado_devuelve_token_renovado()
    {
        var client = await factory.CreateUserClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.True(response.Headers.Contains(TokenService.RefreshHeader));
    }

    // AC-48 (RNF-06)
    [Fact]
    public async Task Token_sin_uso_por_mas_de_24h_responde_401()
    {
        var tokens = factory.Services.GetRequiredService<TokenService>();
        var stale = tokens.CreateToken(1, "x@test.com", Role.Comun, DateTime.UtcNow.AddHours(-24).AddMinutes(-1));
        _client.UseToken(stale);

        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
