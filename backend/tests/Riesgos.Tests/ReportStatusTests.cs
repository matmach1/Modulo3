using System.Net;
using System.Net.Http.Json;
using Riesgos.Api.Domain;
using Riesgos.Api.Endpoints;

namespace Riesgos.Tests;

public class ReportStatusTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Action = "Se reemplazó el cable";
    private const string Note = "Verificado por mantenimiento";

    private static Task<HttpResponseMessage> ChangeStatus(HttpClient client, int id, string status, string? closingAction = null, string? closingNote = null) =>
        client.PatchAsJsonAsync($"/api/reports/{id}/status", new { status, closingAction, closingNote });

    private async Task<(HttpClient Admin, int Id)> ReportInStatus(ReportStatus status)
    {
        var user = await factory.CreateUserClient();
        var id = (await (await user.CreateReport()).ReadReport()).Id;
        var admin = await factory.CreateAdminClient();
        if (status >= ReportStatus.EnProgreso)
            (await ChangeStatus(admin, id, "EnProgreso")).EnsureSuccessStatusCode();
        if (status == ReportStatus.Cerrado)
            (await ChangeStatus(admin, id, "Cerrado", Action, Note)).EnsureSuccessStatusCode();
        return (admin, id);
    }

    private static async Task<ReportDto> Get(HttpClient client, int id) =>
        await (await client.GetAsync($"/api/reports/{id}")).ReadReport();

    // AC-15
    [Fact]
    public async Task Admin_pasa_un_reporte_de_abierto_a_en_progreso()
    {
        var (admin, id) = await ReportInStatus(ReportStatus.Abierto);

        var response = await ChangeStatus(admin, id, "EnProgreso");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ReportStatus.EnProgreso, (await Get(admin, id)).Status);
    }

    // AC-35, AC-18
    [Fact]
    public async Task Admin_cierra_un_reporte_en_progreso_con_accion_y_observacion()
    {
        var (admin, id) = await ReportInStatus(ReportStatus.EnProgreso);
        var before = DateTime.UtcNow;

        var response = await ChangeStatus(admin, id, "Cerrado", Action, Note);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var report = await Get(admin, id);
        Assert.Equal(ReportStatus.Cerrado, report.Status);
        Assert.Equal(Action, report.ClosingAction);
        Assert.Equal(Note, report.ClosingNote);
        Assert.Equal(ApiFactory.AdminEmail, report.ClosedBy);
        Assert.InRange(report.ClosedAt!.Value, before, DateTime.UtcNow);
    }

    // AC-25
    [Theory]
    [InlineData(null, Note)]
    [InlineData(Action, null)]
    [InlineData(" ", Note)]
    public async Task Cerrar_sin_accion_u_observacion_responde_400_y_sigue_en_progreso(string? action, string? note)
    {
        var (admin, id) = await ReportInStatus(ReportStatus.EnProgreso);

        var response = await ChangeStatus(admin, id, "Cerrado", action, note);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ReportStatus.EnProgreso, (await Get(admin, id)).Status);
    }

    // AC-34
    [Fact]
    public async Task Cerrar_directo_desde_abierto_responde_409_y_sigue_abierto()
    {
        var (admin, id) = await ReportInStatus(ReportStatus.Abierto);

        var response = await ChangeStatus(admin, id, "Cerrado", Action, Note);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ReportStatus.Abierto, (await Get(admin, id)).Status);
    }

    // AC-33
    [Theory]
    [InlineData("Abierto")]
    [InlineData("EnProgreso")]
    public async Task Un_reporte_cerrado_no_cambia_de_estado(string status)
    {
        var (admin, id) = await ReportInStatus(ReportStatus.Cerrado);

        var response = await ChangeStatus(admin, id, status);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ReportStatus.Cerrado, (await Get(admin, id)).Status);
    }

    // AC-16
    [Fact]
    public async Task Usuario_comun_no_puede_cambiar_el_estado()
    {
        var owner = await factory.CreateUserClient();
        var id = (await (await owner.CreateReport()).ReadReport()).Id;

        var response = await ChangeStatus(owner, id, "EnProgreso");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(ReportStatus.Abierto, (await Get(owner, id)).Status);
    }
}
