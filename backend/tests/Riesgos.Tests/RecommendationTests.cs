using Riesgos.Api.Domain;

namespace Riesgos.Tests;

public class RecommendationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public RecommendationTests(ApiFactory factory)
    {
        _factory = factory;
        _factory.Classifier.Reset();
    }

    // AC-08, AC-09, AC-30
    [Theory]
    [InlineData(Criticality.Critica, "Detener actividad")]
    [InlineData(Criticality.Alta, "Requiere intervención inmediata")]
    [InlineData(Criticality.Media, null)]
    [InlineData(Criticality.Baja, null)]
    [InlineData(Criticality.Pendiente, null)]
    public async Task La_recomendacion_depende_de_la_criticidad(Criticality criticality, string? expected)
    {
        _factory.Classifier.Result = FakeClassifier.DefaultResult with { Criticality = criticality };
        var client = await _factory.CreateUserClient();

        var report = await (await client.CreateReport()).ReadReport();

        Assert.Equal(expected, report.Recommendation);
    }
}
