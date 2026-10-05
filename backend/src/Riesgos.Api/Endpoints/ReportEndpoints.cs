using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Riesgos.Api.Auth;
using Riesgos.Api.Classification;
using Riesgos.Api.Data;
using Riesgos.Api.Domain;

namespace Riesgos.Api.Endpoints;

public record CreateReportRequest(string? Description, string? Location, Category? SuggestedCategory);

public record ReportDto(
    int Id,
    string Description,
    string Location,
    Category? SuggestedCategory,
    ReportType Type,
    Category Category,
    Criticality Criticality,
    ReportStatus Status,
    DateTime CreatedAt,
    string CreatedBy)
{
    public static ReportDto From(Report r) => new(
        r.Id, r.Description, r.Location, r.SuggestedCategory,
        r.Type, r.Category, r.Criticality, r.Status,
        r.CreatedAt, r.CreatedBy.Email);
}

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").RequireAuthorization();

        // RF-01, RF-17; clasificación: RF-04, RF-07, RF-25, RF-35
        group.MapPost("/", async (CreateReportRequest request, ClaimsPrincipal principal, AppDbContext db, ClassificationService classification) =>
        {
            if (string.IsNullOrWhiteSpace(request.Description) || string.IsNullOrWhiteSpace(request.Location))
                return Results.BadRequest(new { error = "La descripción y la ubicación son obligatorias." });
            if (request.SuggestedCategory == Category.Pendiente)
                return Results.BadRequest(new { error = "La categoría sugerida no es válida." });

            var report = new Report
            {
                Description = request.Description.Trim(),
                Location = request.Location.Trim(),
                SuggestedCategory = request.SuggestedCategory,
                CreatedAt = DateTime.UtcNow,
                CreatedById = principal.GetUserId(),
            };
            // RF-05/RF-06: la categoría automática es la definitiva; la sugerida se conserva aparte.
            var result = await classification.ClassifyOrPendingAsync(report.Description, report.Location);
            report.Type = result.Type;
            report.Category = result.Category;
            report.Criticality = result.Criticality;
            db.Reports.Add(report);
            await db.SaveChangesAsync();
            await db.Entry(report).Reference(r => r.CreatedBy).LoadAsync();

            return Results.Created($"/api/reports/{report.Id}", ReportDto.From(report));
        });
    }
}
