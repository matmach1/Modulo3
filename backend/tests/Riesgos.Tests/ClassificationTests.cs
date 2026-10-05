using System.Net;
using Riesgos.Api.Classification;
using Riesgos.Api.Domain;

namespace Riesgos.Tests;

public class ClassificationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ClassificationTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.Classifier.Reset();
    }

    // AC-40 (y RF-04, RF-07)
    [Fact]
    public async Task Reporte_nuevo_queda_con_la_clasificacion_del_modelo()
    {
        _factory.Classifier.Result = new(ReportType.ActoInseguro, Category.UsoEpp, Criticality.Critica);
        var client = await _factory.CreateUserClient();

        var report = await (await client.CreateReport()).ReadReport();

        Assert.Equal(ReportType.ActoInseguro, report.Type);
        Assert.Equal(Category.UsoEpp, report.Category);
        Assert.Equal(Criticality.Critica, report.Criticality);
    }

    [Fact]
    public async Task El_modelo_recibe_descripcion_y_ubicacion_en_una_sola_llamada()
    {
        var client = await _factory.CreateUserClient();

        await client.CreateReport("Piso mojado sin señalizar", "Depósito");

        Assert.Equal([("Piso mojado sin señalizar", "Depósito")], _factory.Classifier.Calls);
    }

    // AC-56
    [Fact]
    public async Task Si_el_modelo_falla_el_reporte_se_crea_pendiente_de_clasificacion()
    {
        _factory.Classifier.Fail = true;
        var client = await _factory.CreateUserClient();

        var response = await client.CreateReport();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var report = await response.ReadReport();
        Assert.Equal(ReportType.Pendiente, report.Type);
        Assert.Equal(Category.Pendiente, report.Category);
        Assert.Equal(Criticality.Pendiente, report.Criticality);
    }

    // AC-58 (con el tiempo límite acortado en ApiFactory; el valor real se verifica abajo)
    [Fact]
    public async Task Si_el_modelo_no_responde_a_tiempo_el_reporte_se_crea_pendiente_de_clasificacion()
    {
        _factory.Classifier.Delay = TimeSpan.FromSeconds(ApiFactory.ClassifierTimeoutSeconds * 4);
        var client = await _factory.CreateUserClient();

        var response = await client.CreateReport();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(Criticality.Pendiente, (await response.ReadReport()).Criticality);
    }

    [Fact]
    public void El_tiempo_limite_por_defecto_es_10_segundos()
    {
        Assert.Equal(TimeSpan.FromSeconds(10), ClassificationService.DefaultTimeout);
    }
}
