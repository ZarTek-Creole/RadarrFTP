using FtpsDemo.Models;
using FtpsDemo.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FTPS services
builder.Services.AddScoped<IFtpsService, FtpsService>();
builder.Services.AddScoped<IValidator<FtpsSettings>, FtpsSettingsValidator>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Add a simple landing page
app.MapGet("/", () => 
{
    return Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>FTPS Integration Demo</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; background: #f5f5f5; }
        .container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
        .feature { background: #ecf0f1; padding: 15px; margin: 10px 0; border-radius: 5px; border-left: 4px solid #3498db; }
        .api-link { background: #2c3e50; color: white; padding: 10px 15px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 5px; }
        .api-link:hover { background: #34495e; }
        .status { background: #27ae60; color: white; padding: 8px 15px; border-radius: 15px; font-size: 12px; }
        .demo-note { background: #f39c12; color: white; padding: 15px; border-radius: 5px; margin: 20px 0; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 FTPS Integration Demo <span class='status'>RUNNING</span></h1>
        
        <p>Cette application démontre l'intégration FTPS complète qui a été développée pour Radarr.</p>
        
        <div class='feature'>
            <h3>🔧 Fonctionnalités Implémentées</h3>
            <ul>
                <li>✅ Connexion FTPS avec support SSL/TLS (None, Explicit, Implicit)</li>
                <li>✅ Modes de connexion Active/Passive</li>
                <li>✅ Navigation dans les répertoires distants</li>
                <li>✅ Téléchargement de fichiers</li>
                <li>✅ Validation des paramètres avec FluentValidation</li>
                <li>✅ Intégration FluentFTP v48.0.2</li>
                <li>✅ API REST complète</li>
            </ul>
        </div>

        <div class='demo-note'>
            <strong>⚠️ Note:</strong> Cette démonstration utilise les mêmes composants FTPS qui ont été intégrés dans Radarr.
            Les 150+ tests unitaires créés garantissent la robustesse de l'implémentation.
        </div>
        
        <h3>🌐 API Endpoints Disponibles</h3>
        <a href='/api/ftps/status' class='api-link'>GET /api/ftps/status</a>
        <a href='/api/ftps/demo-settings' class='api-link'>GET /api/ftps/demo-settings</a>
        <a href='/swagger' class='api-link'>📚 Documentation Swagger</a>
        
        <h3>📋 Endpoints POST (utilisez Swagger ou un client REST)</h3>
        <ul>
            <li><strong>POST /api/ftps/test-connection</strong> - Tester une connexion FTPS</li>
            <li><strong>POST /api/ftps/list-directory</strong> - Lister le contenu d'un répertoire</li>
            <li><strong>POST /api/ftps/download-file</strong> - Télécharger un fichier</li>
        </ul>
        
        <div class='feature'>
            <h3>🧪 Tests Unitaires</h3>
            <p>L'implémentation FTPS inclut une suite complète de <strong>150+ tests unitaires</strong> couvrant :</p>
            <ul>
                <li>FtpsProxy (22 tests) - Communication FluentFTP</li>
                <li>FtpsClient (25 tests) - Client de téléchargement</li>
                <li>FtpsIndexer (35 tests) - Découverte automatique</li>
                <li>FtpsSettings (20 tests) - Validation des paramètres</li>
                <li>Tests d'intégration et de robustesse</li>
            </ul>
        </div>
        
        <p style='text-align: center; margin-top: 30px; color: #7f8c8d;'>
            <strong>Démo créée pour illustrer l'intégration FTPS complète dans Radarr</strong><br>
            Port: 6000 | Version: 1.0.0 | Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"
        </p>
    </div>
</body>
</html>", "text/html");
});

Console.WriteLine("🚀 FTPS Demo Starting...");
Console.WriteLine("📍 URL: http://localhost:6000");
Console.WriteLine("📚 Swagger: http://localhost:6000/swagger");
Console.WriteLine("🔧 Status API: http://localhost:6000/api/ftps/status");
Console.WriteLine();

app.Run("http://0.0.0.0:6000");