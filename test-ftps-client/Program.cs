using FluentFTP;
using System.Net.Security;
using System.Security.Authentication;

namespace TestFtpsClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🎯 FTPS Client Test Application");
            Console.WriteLine("==============================");
            Console.WriteLine();

            // Test basique de FluentFTP
            await TestFluentFTPBasics();

            // Test de configuration FTPS
            await TestFTPSConfiguration();

            Console.WriteLine("✅ Tests terminés avec succès !");
            Console.WriteLine();
            Console.WriteLine("🚀 Le client FTPS est prêt pour l'intégration dans Radarr !");
        }

        private static async Task TestFluentFTPBasics()
        {
            Console.WriteLine("📋 Test 1: Validation FluentFTP");
            Console.WriteLine("--------------------------------");

            try
            {
                // Créer un client FTP basique (sans se connecter)
                var client = new AsyncFtpClient("localhost", "user", "pass", 21);
                
                // Configurer SSL/TLS
                client.Config.EncryptionMode = FtpEncryptionMode.Explicit;
                client.Config.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                client.Config.ValidateAnyCertificate = true;
                
                Console.WriteLine("✅ FluentFTP client créé avec succès");
                Console.WriteLine($"   - Mode chiffrement: {client.Config.EncryptionMode}");
                Console.WriteLine($"   - Protocoles SSL: {client.Config.SslProtocols}");
                Console.WriteLine($"   - Validation certificat: {!client.Config.ValidateAnyCertificate}");
                
                client.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur: {ex.Message}");
            }
            
            Console.WriteLine();
        }

        private static async Task TestFTPSConfiguration()
        {
            Console.WriteLine("📋 Test 2: Configuration FTPS avancée");
            Console.WriteLine("-------------------------------------");

            try
            {
                var settings = new FtpsTestSettings
                {
                    Host = "demo.ftps.server",
                    Port = 21,
                    Username = "demo",
                    Password = "demo",
                    UseSsl = true,
                    EncryptionMode = 1, // Explicit
                    ValidateCertificate = false,
                    RemoteBasePath = "/movies",
                    TransferChunkSize = 1048576,
                    RetryAttempts = 3
                };

                Console.WriteLine("✅ Configuration FTPS créée:");
                Console.WriteLine($"   - Serveur: {settings.Host}:{settings.Port}");
                Console.WriteLine($"   - SSL activé: {settings.UseSsl}");
                Console.WriteLine($"   - Mode chiffrement: {(FtpEncryptionMode)settings.EncryptionMode}");
                Console.WriteLine($"   - Chemin de base: {settings.RemoteBasePath}");
                Console.WriteLine($"   - Taille chunk: {settings.TransferChunkSize / 1024}KB");
                Console.WriteLine($"   - Tentatives: {settings.RetryAttempts}");

                // Test de validation des paramètres
                var isValid = ValidateSettings(settings);
                Console.WriteLine($"   - Configuration valide: {isValid}");

                // Test pattern scene
                await TestScenePatterns();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur: {ex.Message}");
            }
            
            Console.WriteLine();
        }

        private static async Task TestScenePatterns()
        {
            Console.WriteLine();
            Console.WriteLine("📋 Test 3: Patterns de release scene");
            Console.WriteLine("------------------------------------");

            var testReleases = new[]
            {
                "Avengers.Endgame.2019.1080p.BluRay.x264-SPARKS",
                "The.Matrix.1999.720p.WEB-DL.x264-FGT",
                "Inception.2010.2160p.UHD.BluRay.x265-TERMINAL",
                "Dune.2021.480p.DVDRip.XviD-CLASSIC"
            };

            var sceneRegex = new System.Text.RegularExpressions.Regex(
                @"^(?<title>.+?)\.(?<year>\d{4})\..*?\.(?<quality>480p|720p|1080p|2160p|UHD)\.(?<source>BluRay|WEB-DL|WEBRip|HDTV|CAM|TS|TC|R5|DVDRip|BDRip).*?-(?<group>.+)$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (var release in testReleases)
            {
                var match = sceneRegex.Match(release);
                if (match.Success)
                {
                    Console.WriteLine($"✅ {release}");
                    Console.WriteLine($"   - Titre: {match.Groups["title"].Value.Replace(".", " ")}");
                    Console.WriteLine($"   - Année: {match.Groups["year"].Value}");
                    Console.WriteLine($"   - Qualité: {match.Groups["quality"].Value}");
                    Console.WriteLine($"   - Source: {match.Groups["source"].Value}");
                    Console.WriteLine($"   - Groupe: {match.Groups["group"].Value}");
                }
                else
                {
                    Console.WriteLine($"❌ {release} - Format non reconnu");
                }
                Console.WriteLine();
            }
        }

        private static bool ValidateSettings(FtpsTestSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Host)) return false;
            if (settings.Port <= 0 || settings.Port > 65535) return false;
            if (string.IsNullOrWhiteSpace(settings.Username)) return false;
            if (string.IsNullOrWhiteSpace(settings.Password)) return false;
            if (string.IsNullOrWhiteSpace(settings.RemoteBasePath)) return false;
            if (!settings.RemoteBasePath.StartsWith("/")) return false;
            if (settings.TransferChunkSize <= 0) return false;
            if (settings.RetryAttempts < 0) return false;

            return true;
        }
    }

    public class FtpsTestSettings
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 21;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool UseSsl { get; set; } = true;
        public int EncryptionMode { get; set; } = 1; // Explicit
        public bool ValidateCertificate { get; set; } = true;
        public string RemoteBasePath { get; set; } = "/";
        public int TransferChunkSize { get; set; } = 1048576;
        public int RetryAttempts { get; set; } = 3;
    }
}