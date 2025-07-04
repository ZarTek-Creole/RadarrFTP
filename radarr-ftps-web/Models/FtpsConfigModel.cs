using System.ComponentModel.DataAnnotations;

namespace RadarrFtpsWeb.Models;

public class FtpsConfigModel
{
    [Required]
    [Display(Name = "Nom du serveur")]
    public string Name { get; set; } = "";

    [Required]
    [Display(Name = "Serveur FTPS")]
    public string Host { get; set; } = "";

    [Required]
    [Range(1, 65535)]
    [Display(Name = "Port")]
    public int Port { get; set; } = 21;

    [Required]
    [Display(Name = "Nom d'utilisateur")]
    public string Username { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = "";

    [Display(Name = "Utiliser SSL/TLS")]
    public bool UseSsl { get; set; } = true;

    [Display(Name = "Mode de chiffrement")]
    public EncryptionMode EncryptionMode { get; set; } = EncryptionMode.Explicit;

    [Display(Name = "Valider le certificat")]
    public bool ValidateCertificate { get; set; } = false;

    [Display(Name = "Chemin de base distant")]
    public string RemoteBasePath { get; set; } = "/";

    [Display(Name = "Timeout (secondes)")]
    [Range(10, 300)]
    public int Timeout { get; set; } = 30;

    [Display(Name = "Tentatives de reconnexion")]
    [Range(1, 10)]
    public int RetryAttempts { get; set; } = 3;
}

public enum EncryptionMode
{
    [Display(Name = "Explicit FTPS")]
    Explicit = 1,
    
    [Display(Name = "Implicit FTPS")]
    Implicit = 2
}

public class FtpsTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<string> Files { get; set; } = new();
    public TimeSpan ConnectionTime { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}