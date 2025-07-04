using Microsoft.AspNetCore.Mvc;
using RadarrFtpsWeb.Models;
using RadarrFtpsWeb.Services;
using System.Diagnostics;

namespace RadarrFtpsWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly FtpsTestService _ftpsService;

    public HomeController(ILogger<HomeController> logger, FtpsTestService ftpsService)
    {
        _logger = logger;
        _ftpsService = ftpsService;
    }

    public IActionResult Index()
    {
        var model = new FtpsConfigModel
        {
            Name = "Serveur de Test",
            Host = "test.rebex.net",
            Port = 21,
            Username = "demo",
            Password = "password",
            UseSsl = true,
            EncryptionMode = EncryptionMode.Explicit,
            ValidateCertificate = false,
            RemoteBasePath = "/",
            Timeout = 30,
            RetryAttempts = 3
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> TestConnection(FtpsConfigModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        try
        {
            var result = await _ftpsService.TestConnectionAsync(model);
            ViewBag.TestResult = result;
            ViewBag.ShowResult = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du test de connexion");
            ViewBag.TestResult = new FtpsTestResult
            {
                Success = false,
                Message = $"❌ Erreur: {ex.Message}"
            };
            ViewBag.ShowResult = true;
        }

        return View("Index", model);
    }

    [HttpGet]
    public IActionResult Presets(string preset)
    {
        var model = preset switch
        {
            "rebex" => new FtpsConfigModel
            {
                Name = "Rebex Test Server",
                Host = "test.rebex.net",
                Port = 21,
                Username = "demo",
                Password = "password",
                UseSsl = true,
                EncryptionMode = EncryptionMode.Explicit,
                ValidateCertificate = false,
                RemoteBasePath = "/",
                Timeout = 30,
                RetryAttempts = 3
            },
            "filezilla" => new FtpsConfigModel
            {
                Name = "FileZilla Test Server",
                Host = "demo.filezilla-project.org",
                Port = 21,
                Username = "demo",
                Password = "demo",
                UseSsl = true,
                EncryptionMode = EncryptionMode.Explicit,
                ValidateCertificate = false,
                RemoteBasePath = "/",
                Timeout = 30,
                RetryAttempts = 3
            },
            _ => new FtpsConfigModel()
        };

        return Json(model);
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}