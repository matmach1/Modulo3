namespace Riesgos.Api.Domain;

/// <summary>RF-09: acción preventiva predefinida según la criticidad.</summary>
public static class Recommendations
{
    public static string? For(Criticality criticality) => criticality switch
    {
        Criticality.Critica => "Detener actividad",
        Criticality.Alta => "Requiere intervención inmediata",
        _ => null,
    };
}
