using FluentFTP;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Authentication;
using RadarrFtpsWeb.Models;
using System.Text.RegularExpressions;

namespace RadarrFtpsWeb.Services;

public class FtpsTestService
{
    private readonly ILogger<FtpsTestService> _logger;

    public FtpsTestService(ILogger<FtpsTestService> logger)
    {
        _logger = logger;
    }

    public async Task<FtpsTestResult> TestConnectionAsync(FtpsConfigModel config)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new FtpsTestResult();

        try
        {
            _logger.LogInformation("🔍 Test de connexion FTPS vers {Host}:{Port}", config.Host, config.Port);

            using var client = new AsyncFtpClient(config.Host, config.Username, config.Password, config.Port);
            
            // Configuration SSL/TLS
            if (config.UseSsl)
            {
                client.Config.EncryptionMode = config.EncryptionMode == EncryptionMode.Explicit 
                    ? FtpEncryptionMode.Explicit 
                    : FtpEncryptionMode.Implicit;
                    
                client.Config.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                client.Config.ValidateAnyCertificate = !config.ValidateCertificate;
            }
            else
            {
                client.Config.EncryptionMode = FtpEncryptionMode.None;
            }

            // Configuration générale
            client.Config.ConnectTimeout = config.Timeout * 1000;
            client.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;
            client.Config.RetryAttempts = config.RetryAttempts;

            // Test de connexion
            await client.Connect();
            _logger.LogInformation("✅ Connexion établie avec succès");

            // Test changement de répertoire
            if (!string.IsNullOrEmpty(config.RemoteBasePath) && config.RemoteBasePath != "/")
            {
                var exists = await client.DirectoryExists(config.RemoteBasePath);
                if (exists)
                {
                    await client.SetWorkingDirectory(config.RemoteBasePath);
                    _logger.LogInformation("✅ Répertoire de base accessible: {Path}", config.RemoteBasePath);
                }
                else
                {
                    _logger.LogWarning("⚠️ Répertoire de base non trouvé: {Path}", config.RemoteBasePath);
                }
            }

            // Lister les fichiers du répertoire courant
            var files = await client.GetListing();
            var fileList = files.Take(20).Select(f => $"{f.Type} - {f.Name} ({f.Size} bytes)").ToList();

            stopwatch.Stop();

            // Analyser les releases scene si des fichiers sont trouvés
            var sceneFiles = AnalyzeSceneReleases(files.Where(f => f.Type == FtpObjectType.File).Select(f => f.Name));

            result.Success = true;
            result.Message = $"✅ Connexion réussie en {stopwatch.ElapsedMilliseconds}ms";
            result.Files = fileList;
            result.ConnectionTime = stopwatch.Elapsed;
            result.Details = new Dictionary<string, object>
            {
                ["ServerType"] = client.ServerType.ToString(),
                ["ConnectionMode"] = client.Config.EncryptionMode.ToString(),
                ["FilesCount"] = files.Length,
                ["SceneFilesFound"] = sceneFiles.Count,
                ["WorkingDirectory"] = await client.GetWorkingDirectory()
            };

            if (sceneFiles.Any())
            {
                result.Details["SceneReleases"] = sceneFiles;
            }

            await client.Disconnect();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erreur lors du test de connexion FTPS");
            
            result.Success = false;
            result.Message = $"❌ Erreur: {ex.Message}";
            result.ConnectionTime = stopwatch.Elapsed;
            result.Details["ErrorType"] = ex.GetType().Name;
            result.Details["ErrorDetails"] = ex.ToString();
        }

        return result;
    }

    private List<SceneRelease> AnalyzeSceneReleases(IEnumerable<string> fileNames)
    {
        var sceneRegex = new Regex(
            @"^(?<title>.+?)\.(?<year>\d{4})\..*?\.(?<quality>480p|720p|1080p|2160p|UHD)\.(?<source>BluRay|WEB-DL|WEBRip|HDTV|CAM|TS|TC|R5|DVDRip|BDRip).*?-(?<group>.+)$",
            RegexOptions.IgnoreCase);

        var releases = new List<SceneRelease>();

        foreach (var fileName in fileNames)
        {
            var match = sceneRegex.Match(fileName);
            if (match.Success)
            {
                releases.Add(new SceneRelease
                {
                    FileName = fileName,
                    Title = match.Groups["title"].Value.Replace(".", " "),
                    Year = int.Parse(match.Groups["year"].Value),
                    Quality = match.Groups["quality"].Value,
                    Source = match.Groups["source"].Value,
                    Group = match.Groups["group"].Value,
                    Score = CalculateScore(match.Groups["quality"].Value, match.Groups["source"].Value)
                });
            }
        }

        return releases.OrderByDescending(r => r.Score).ToList();
    }

    private int CalculateScore(string quality, string source)
    {
        var score = 0;

        // Score basé sur la qualité
        score += quality switch
        {
            "2160p" or "UHD" => 100,
            "1080p" => 80,
            "720p" => 60,
            "480p" => 40,
            _ => 20
        };

        // Score basé sur la source
        score += source switch
        {
            "BluRay" => 50,
            "WEB-DL" => 40,
            "WEBRip" => 30,
            "DVDRip" or "BDRip" => 25,
            "HDTV" => 20,
            _ => 10
        };

        return score;
    }
}

public class SceneRelease
{
    public string FileName { get; set; } = "";
    public string Title { get; set; } = "";
    public int Year { get; set; }
    public string Quality { get; set; } = "";
    public string Source { get; set; } = "";
    public string Group { get; set; } = "";
    public int Score { get; set; }
}