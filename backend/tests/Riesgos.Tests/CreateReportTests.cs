using System.Net;
using Riesgos.Api.Domain;

namespace Riesgos.Tests;

public class CreateReportTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // AC-27, AC-17
    [Fact]
    public async Task Reporte_valido_responde_201_abierto_con_datos_de_creacion()
    {
        var email = TestAuth.NewEmail();
        var client = await factory.CreateUserClient(email);
        var before = DateTime.UtcNow;

        var response = await client.CreateReport();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var report = await response.ReadReport();
        Assert.Equal(ReportStatus.Abierto, report.Status);
        Assert.Equal(email, report.CreatedBy);
        Assert.InRange(report.CreatedAt, before, DateTime.UtcNow);
    }

    // AC-01, AC-28
    [Theory]
    [InlineData("", "Planta 1")]
    [InlineData("   ", "Planta 1")]
    [InlineData("Cable pelado", "")]
    public async Task Descripcion_o_ubicacion_vacia_responde_400(string description, string location)
    {
        var client = await factory.CreateUserClient();

        var response = await client.CreateReport(description, location);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Categoria_sugerida_inexistente_responde_400()
    {
        var client = await factory.CreateUserClient();

        var response = await client.CreateReport(suggestedCategory: "Inventada");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Sin_autenticacion_responde_401()
    {
        var response = await factory.CreateClient().CreateReport();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
