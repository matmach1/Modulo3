using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Riesgos.Tests;

public record LoginResponse(string Token, string Email, string Role);

public static class TestAuth
{
    public static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

    public static async Task Register(this HttpClient client, string email, string password = "secreta123")
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        response.EnsureSuccessStatusCode();
    }

    public static async Task<string> Login(this HttpClient client, string email, string password = "secreta123")
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
    }

    public static void UseToken(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    /// <summary>Crea un cliente autenticado como un usuario común nuevo.</summary>
    public static async Task<HttpClient> CreateUserClient(this ApiFactory factory, string? email = null)
    {
        var client = factory.CreateClient();
        email ??= NewEmail();
        await client.Register(email);
        client.UseToken(await client.Login(email));
        return client;
    }

    /// <summary>Crea un cliente autenticado como el administrador del seed.</summary>
    public static async Task<HttpClient> CreateAdminClient(this ApiFactory factory)
    {
        var client = factory.CreateClient();
        client.UseToken(await client.Login(ApiFactory.AdminEmail, ApiFactory.AdminPassword));
        return client;
    }
}
