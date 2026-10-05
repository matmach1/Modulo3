using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Riesgos.Api.Classification;

namespace Riesgos.Tests;

/// <summary>
/// Levanta la API contra una base SQLite en memoria propia de cada instancia.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@test.com";
    public const string AdminPassword = "admin-secreta";

    /// <summary>Tiempo límite corto para que el test de timeout no tarde 10 s.</summary>
    public const double ClassifierTimeoutSeconds = 0.5;

    public FakeClassifier Classifier { get; } = new();

    private readonly string _connectionString = $"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared";
    private readonly SqliteConnection _keepAlive;

    public ApiFactory()
    {
        // La base en memoria vive mientras haya al menos una conexión abierta.
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _connectionString);
        builder.UseSetting("JWT_KEY", "clave-de-pruebas-de-al-menos-32-caracteres");
        builder.UseSetting("ADMIN_EMAIL", AdminEmail);
        builder.UseSetting("ADMIN_PASSWORD", AdminPassword);
        builder.UseSetting("Classifier:TimeoutSeconds", ClassifierTimeoutSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        builder.ConfigureTestServices(services => services.AddSingleton<IClassifier>(Classifier));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _keepAlive.Dispose();
    }
}
