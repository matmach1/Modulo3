using System.Net.Http.Json;

namespace Riesgos.Tests;

public class AdminSeedTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // AC-44 (el acceso al listado global se verifica en ReportListTests)
    [Fact]
    public async Task Admin_del_seed_puede_autenticarse_con_rol_admin()
    {
        var client = await factory.CreateAdminClient();

        var me = await client.GetFromJsonAsync<LoginResponse>("/api/auth/me");

        Assert.Equal("Admin", me!.Role);
    }
}
