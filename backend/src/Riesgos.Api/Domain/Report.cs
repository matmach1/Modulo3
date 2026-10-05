namespace Riesgos.Api.Domain;

public enum ReportStatus
{
    Abierto,
    EnProgreso,
    Cerrado,
}

// En los tres enums de clasificación, Pendiente = "Pendiente de clasificación" (RF-35).
public enum ReportType
{
    Pendiente,
    ActoInseguro,
    CondicionInsegura,
    CasiAccidente,
    RiesgoOperativo,
    Otro,
}

public enum Category
{
    Pendiente,
    ProblemaInfraestructura,
    RiesgoElectrico,
    UsoEpp,
    Otro,
}

public enum Criticality
{
    Pendiente,
    Baja,
    Media,
    Alta,
    Critica,
}

public class Report
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public Category? SuggestedCategory { get; set; }

    public ReportType Type { get; set; } = ReportType.Pendiente;
    public Category Category { get; set; } = Category.Pendiente;
    public Criticality Criticality { get; set; } = Criticality.Pendiente;

    public ReportStatus Status { get; set; } = ReportStatus.Abierto;

    public DateTime CreatedAt { get; set; }
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
}
