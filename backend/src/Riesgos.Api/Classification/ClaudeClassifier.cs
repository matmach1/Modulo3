using System.Text.Json;
using Anthropic;
using Anthropic.Models.Beta;
using Anthropic.Models.Beta.Messages;
using Anthropic.Models.Messages;
using Riesgos.Api.Domain;
using Category = Riesgos.Api.Domain.Category;
using MessageCreateParams = Anthropic.Models.Beta.Messages.MessageCreateParams;
using Effort = Anthropic.Models.Beta.Messages.Effort;
using Role = Anthropic.Models.Beta.Messages.Role;

namespace Riesgos.Api.Classification;

/// <summary>
/// RF-04, RF-07, RF-25, RNF-01: una única llamada a Claude devuelve tipo, categoría
/// y criticidad, con salida estructurada y kb.md como contexto.
/// </summary>
public class ClaudeClassifier : IClassifier
{
    private const string DefaultModel = "claude-opus-5-5";

    private readonly AnthropicClient _client;
    private readonly string _model;
    private readonly string _systemPrompt;

    public ClaudeClassifier(IConfiguration configuration, IWebHostEnvironment environment)
    {
        // RNF-04: la API key solo se lee de la variable de entorno API_KEY.
        var apiKey = configuration["API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Falta la variable de entorno API_KEY.");

        _client = new AnthropicClient { ApiKey = apiKey };
        _model = configuration["CLAUDE_MODEL"] is { Length: > 0 } model ? model : DefaultModel;

        var knowledgeBase = File.ReadAllText(Path.Combine(environment.ContentRootPath, "kb.md"));
        _systemPrompt =
            "Sos un especialista en Seguridad e Higiene laboral. Clasificás reportes de riesgo " +
            "escritos por empleados según la base de conocimiento que sigue. Respondé solo con " +
            "los valores exactos permitidos por el esquema.\n\n" + knowledgeBase;
    }

    public async Task<Classification> ClassifyAsync(string description, string location, CancellationToken cancellationToken)
    {
        var response = await _client.Beta.Messages.Create(new MessageCreateParams
        {
            Model = _model,
            MaxTokens = 1024,
            // Si el modelo declina por sus salvaguardas, el servidor reintenta con otro modelo.
            Betas = [AnthropicBeta.ServerSideFallback2026_07_01],
            Fallbacks = new BetaFallbacksParam(JsonSerializer.SerializeToElement("default")),
            System = new List<BetaTextBlockParam> { new() { Text = _systemPrompt, CacheControl = new BetaCacheControlEphemeral() } },
            OutputConfig = new BetaOutputConfig
            {
                Effort = Effort.Low,
                Format = new BetaJsonOutputFormat { Schema = Schema },
            },
            Messages = [new() { Role = Role.User, Content = $"Descripción: {description}\nUbicación: {location}" }],
        }, cancellationToken);

        if (response.StopReason == "refusal")
            throw new InvalidOperationException("El modelo declinó clasificar el reporte.");

        var json = response.Content.Select(b => b.Value).OfType<BetaTextBlock>().Single().Text;
        var result = JsonSerializer.Deserialize<ClaudeResult>(json)
            ?? throw new InvalidOperationException("Respuesta vacía del modelo.");

        return new Classification(
            Enum.Parse<ReportType>(result.Type),
            Enum.Parse<Category>(result.Category),
            Enum.Parse<Criticality>(result.Criticality));
    }

    private static string[] NamesExceptPending<T>() where T : struct, Enum =>
        Enum.GetNames<T>().Where(n => n != "Pendiente").ToArray();

    private static readonly Dictionary<string, JsonElement> Schema = new()
    {
        ["type"] = JsonSerializer.SerializeToElement("object"),
        ["properties"] = JsonSerializer.SerializeToElement(new Dictionary<string, object>
        {
            ["type"] = new { type = "string", @enum = NamesExceptPending<ReportType>() },
            ["category"] = new { type = "string", @enum = NamesExceptPending<Category>() },
            ["criticality"] = new { type = "string", @enum = NamesExceptPending<Criticality>() },
        }),
        ["required"] = JsonSerializer.SerializeToElement(new[] { "type", "category", "criticality" }),
        ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
    };

    private record ClaudeResult(
        [property: System.Text.Json.Serialization.JsonPropertyName("type")] string Type,
        [property: System.Text.Json.Serialization.JsonPropertyName("category")] string Category,
        [property: System.Text.Json.Serialization.JsonPropertyName("criticality")] string Criticality);
}
