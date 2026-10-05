using Riesgos.Api.Classification;
using Riesgos.Api.Domain;

namespace Riesgos.Tests;

/// <summary>Clasificador simulado: devuelve un resultado fijo, falla o se demora según se configure.</summary>
public class FakeClassifier : IClassifier
{
    public static readonly Classification DefaultResult = new(ReportType.CondicionInsegura, Category.RiesgoElectrico, Criticality.Alta);

    public Classification Result { get; set; } = DefaultResult;
    public bool Fail { get; set; }
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;
    public List<(string Description, string Location)> Calls { get; } = [];

    public async Task<Classification> ClassifyAsync(string description, string location, CancellationToken cancellationToken)
    {
        Calls.Add((description, location));
        if (Delay > TimeSpan.Zero)
            await Task.Delay(Delay, cancellationToken);
        if (Fail)
            throw new HttpRequestException("Claude API no disponible (simulado)");
        return Result;
    }

    public void Reset()
    {
        Result = DefaultResult;
        Fail = false;
        Delay = TimeSpan.Zero;
        Calls.Clear();
    }
}
