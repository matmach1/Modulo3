using Riesgos.Api.Domain;

namespace Riesgos.Api.Classification;

public record Classification(ReportType Type, Category Category, Criticality Criticality)
{
    public static readonly Classification Pending = new(ReportType.Pendiente, Category.Pendiente, Criticality.Pendiente);
}

/// <summary>
/// Clasifica un reporte a partir de su texto. RF-21: la firma no recibe imágenes,
/// así que el modelo nunca puede verlas.
/// </summary>
public interface IClassifier
{
    Task<Classification> ClassifyAsync(string description, string location, CancellationToken cancellationToken);
}
