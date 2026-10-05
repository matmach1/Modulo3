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
    string CreatedBy,
    string? Recommendation)
{
    public static ReportDto From(Report r) => new(
        r.Id, r.Description, r.Location, r.SuggestedCategory,
        r.Type, r.Category, r.Criticality, r.Status,
        r.CreatedAt, r.CreatedBy.Email,
        Recommendations.For(r.Criticality));
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int Size, int Total);

public static class ReportEndpoints
{
    public const int MaxPageSize = 100;

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

        // RF-02a, RF-02b, RF-02c, RF-02d: listado global, solo administrador.
        group.MapGet("/", async (AppDbContext db, int page = 1, int size = 20) =>
        {
            if (page < 1 || size < 1 || size > MaxPageSize)
                return Results.BadRequest(new { error = $"page debe ser ≥ 1 y size entre 1 y {MaxPageSize}." });

            var total = await db.Reports.CountAsync();
            var items = await NewestFirst(db.Reports)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            return Results.Ok(new PagedResult<ReportDto>(items.Select(ReportDto.From).ToList(), page, size, total));
        }).RequireAuthorization(AuthSetup.AdminPolicy);

        // RF-31: el creador ve sus reportes; el administrador, cualquiera.
        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal principal, AppDbContext db) =>
        {
            var report = await db.Reports.Include(r => r.CreatedBy).SingleOrDefaultAsync(r => r.Id == id);
            if (report is null)
                return Results.NotFound();
            if (!CanView(principal, report))
                return Results.Forbid();
            return Results.Ok(ReportDto.From(report));
        });

        // RF-03: cada usuario ve solo los reportes que creó.
        group.MapGet("/mine", async (ClaimsPrincipal principal, AppDbContext db) =>
        {
            var userId = principal.GetUserId();
            var items = await NewestFirst(db.Reports.Where(r => r.CreatedById == userId)).ToListAsync();
            return Results.Ok(items.Select(ReportDto.From));
        });
    }

    private static bool CanView(ClaimsPrincipal principal, Report report) =>
        principal.IsInRole(nameof(Role.Admin)) || report.CreatedById == principal.GetUserId();

    private static IQueryable<Report> NewestFirst(IQueryable<Report> reports) =>
        reports.Include(r => r.CreatedBy).OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);
}
