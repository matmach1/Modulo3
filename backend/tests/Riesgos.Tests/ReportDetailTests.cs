using System.Net;

namespace Riesgos.Tests;

public class ReportDetailTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // AC-50, AC-17
    [Fact]
    public async Task El_creador_ve_el_detalle_de_su_reporte()
    {
        var email = TestAuth.NewEmail();
        var owner = await factory.CreateUserClient(email);
        var created = await (await owner.CreateReport("Escalera sin baranda", "Nave 2")).ReadReport();

        var response = await owner.GetAsync($"/api/reports/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await response.ReadReport();
        Assert.Equal("Escalera sin baranda", report.Description);
        Assert.Equal(email, report.CreatedBy);
        Assert.Equal(created.CreatedAt, report.CreatedAt, TimeSpan.FromMilliseconds(1));
    }

    // AC-51
    [Fact]
    public async Task El_admin_ve_el_detalle_de_cualquier_reporte()
    {
        var owner = await factory.CreateUserClient();
        var created = await (await owner.CreateReport()).ReadReport();
        var admin = await factory.CreateAdminClient();

        var response = await admin.GetAsync($"/api/reports/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // AC-04
    [Fact]
    public async Task Otro_usuario_comun_recibe_403_y_no_ve_el_reporte()
    {
        var owner = await factory.CreateUserClient();
        var other = await factory.CreateUserClient();
        var created = await (await owner.CreateReport("Dato privado de A")).ReadReport();

        var response = await other.GetAsync($"/api/reports/{created.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.DoesNotContain("Dato privado de A", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Reporte_inexistente_responde_404()
    {
        var admin = await factory.CreateAdminClient();

        var response = await admin.GetAsync("/api/reports/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
