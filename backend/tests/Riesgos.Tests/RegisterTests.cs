using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Riesgos.Api.Data;
using Riesgos.Api.Domain;

namespace Riesgos.Tests;

public class RegisterTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

    private async Task<User> FindUser(string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Users.SingleAsync(u => u.Email == email);
    }

    [Fact]
    public async Task Registro_con_email_nuevo_crea_usuario_comun()
    {
        var email = NewEmail();

        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "secreta123" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(Role.Comun, (await FindUser(email)).Role);
    }

    // AC-31
    [Fact]
    public async Task Registro_con_email_existente_responde_409_y_no_duplica()
    {
        var email = NewEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "secreta123" });

        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email = email.ToUpperInvariant(), password = "otra456" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await db.Users.CountAsync(u => u.Email == email));
    }

    [Theory]
    [InlineData("", "secreta123")]
    [InlineData("a@test.com", "")]
    public async Task Registro_sin_email_o_contraseña_responde_400(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email, password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // AC-47 (RNF-05)
    [Fact]
    public async Task La_contraseña_se_guarda_como_hash_bcrypt()
    {
        var email = NewEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "secreta123" });

        var user = await FindUser(email);

        Assert.NotEqual("secreta123", user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("secreta123", user.PasswordHash));
    }
}
