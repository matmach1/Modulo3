using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Riesgos.Api.Endpoints;

namespace Riesgos.Tests;

public static class ReportJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<ReportDto> ReadReport(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ReportDto>(Options))!;

    public static async Task<T> Read<T>(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Options))!;

    public static Task<HttpResponseMessage> CreateReport(
        this HttpClient client, string description = "Cable pelado en el tablero", string location = "Planta 1", string? suggestedCategory = null) =>
        client.PostAsJsonAsync("/api/reports", new { description, location, suggestedCategory });
}
