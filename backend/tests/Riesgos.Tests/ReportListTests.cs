using System.Net;
using Riesgos.Api.Domain;
using Riesgos.Api.Endpoints;

namespace Riesgos.Tests;

public class ReportListTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<PagedResult<ReportDto>> ListAll(HttpClient admin, int page, int size)
    {
        var response = await admin.GetAsync($"/api/reports?page={page}&size={size}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Read<PagedResult<ReportDto>>();
    }

    // AC-02, AC-44
    [Fact]
    public async Task Admin_lista_paginado_con_estado()
    {
        var user = await factory.CreateUserClient();
        for (var i = 0; i < 5; i++)
            await user.CreateReport($"Reporte {i}");
        var admin = await factory.CreateAdminClient();

        var firstPage = await ListAll(admin, page: 1, size: 2);
        var secondPage = await ListAll(admin, page: 2, size: 2);

        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(2, secondPage.Items.Count);
        Assert.Empty(firstPage.Items.Select(r => r.Id).Intersect(secondPage.Items.Select(r => r.Id)));
        Assert.All(firstPage.Items, r => Assert.Equal(ReportStatus.Abierto, r.Status));
        Assert.True(firstPage.Total >= 5);
    }

    // AC-49
    [Fact]
    public async Task Admin_lista_del_mas_reciente_al_mas_antiguo()
    {
        var user = await factory.CreateUserClient();
        var created = new List<int>();
        for (var i = 0; i < 3; i++)
            created.Add((await (await user.CreateReport($"Orden {i}")).ReadReport()).Id);
        var admin = await factory.CreateAdminClient();

        var items = (await ListAll(admin, page: 1, size: ReportEndpoints.MaxPageSize)).Items;

        var positions = created.Select(id => items.ToList().FindIndex(r => r.Id == id)).ToList();
        Assert.Equal(positions.OrderByDescending(p => p), positions);
        Assert.Equal(items.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id).Select(r => r.Id), items.Select(r => r.Id));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, ReportEndpoints.MaxPageSize + 1)]
    public async Task Parametros_de_pagina_invalidos_responden_400(int page, int size)
    {
        var admin = await factory.CreateAdminClient();

        var response = await admin.GetAsync($"/api/reports?page={page}&size={size}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // AC-03, AC-42
    [Fact]
    public async Task Usuario_comun_no_accede_al_listado_global()
    {
        var user = await factory.CreateUserClient();

        var response = await user.GetAsync("/api/reports");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // AC-12
    [Fact]
    public async Task Sin_autenticacion_el_listado_responde_401()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, response.Content.Headers.ContentLength ?? 0);
    }

    // AC-29
    [Fact]
    public async Task Usuario_comun_lista_solo_sus_reportes()
    {
        var userA = await factory.CreateUserClient();
        var userB = await factory.CreateUserClient();
        var reportA = await (await userA.CreateReport("De A")).ReadReport();
        await userB.CreateReport("De B");

        var mine = await (await userA.GetAsync("/api/reports/mine")).Read<List<ReportDto>>();

        Assert.Equal([reportA.Id], mine.Select(r => r.Id));
    }
}
