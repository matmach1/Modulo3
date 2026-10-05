namespace Riesgos.Api.Classification;

/// <summary>
/// RF-35/RF-37: si el modelo falla o no responde a tiempo, el reporte queda
/// "Pendiente de clasificación" en lugar de perderse.
/// </summary>
public class ClassificationService(IClassifier classifier, IConfiguration configuration, ILogger<ClassificationService> logger)
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private TimeSpan Timeout =>
        double.TryParse(configuration["Classifier:TimeoutSeconds"], System.Globalization.CultureInfo.InvariantCulture, out var seconds)
            ? TimeSpan.FromSeconds(seconds)
            : DefaultTimeout;

    public async Task<Classification> ClassifyOrPendingAsync(string description, string location)
    {
        using var cts = new CancellationTokenSource(Timeout);
        try
        {
            return await classifier.ClassifyAsync(description, location, cts.Token).WaitAsync(cts.Token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo clasificar el reporte: queda Pendiente de clasificación.");
            return Classification.Pending;
        }
    }
}
