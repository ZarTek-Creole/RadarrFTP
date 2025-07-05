using Microsoft.AspNetCore.Mvc;
using FtpsDemo.Models;
using FtpsDemo.Services;
using FluentValidation;

namespace FtpsDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FtpsController : ControllerBase
    {
        private readonly IFtpsService _ftpsService;
        private readonly IValidator<FtpsSettings> _validator;

        public FtpsController(IFtpsService ftpsService, IValidator<FtpsSettings> validator)
        {
            _ftpsService = ftpsService;
            _validator = validator;
        }

        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] FtpsSettings settings)
        {
            var validationResult = await _validator.ValidateAsync(settings);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var success = await _ftpsService.TestConnectionAsync(settings);
            
            return Ok(new 
            { 
                success,
                message = success ? "Connection successful!" : "Connection failed",
                timestamp = DateTime.Now
            });
        }

        [HttpPost("list-directory")]
        public async Task<IActionResult> ListDirectory([FromBody] FtpsListRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request.Settings);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var items = await _ftpsService.GetDirectoryListingAsync(request.Settings, request.Path);
            
            return Ok(new 
            { 
                path = request.Path,
                items,
                count = items.Count,
                timestamp = DateTime.Now
            });
        }

        [HttpPost("download-file")]
        public async Task<IActionResult> DownloadFile([FromBody] FtpsDownloadRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request.Settings);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var tempFile = Path.GetTempFileName();
            var success = await _ftpsService.DownloadFileAsync(request.Settings, request.RemotePath, tempFile);
            
            if (success && System.IO.File.Exists(tempFile))
            {
                var fileInfo = new FileInfo(tempFile);
                System.IO.File.Delete(tempFile); // Clean up
                
                return Ok(new 
                { 
                    success = true,
                    remotePath = request.RemotePath,
                    downloadedSize = fileInfo.Length,
                    message = "File downloaded successfully",
                    timestamp = DateTime.Now
                });
            }
            else
            {
                return Ok(new 
                { 
                    success = false,
                    remotePath = request.RemotePath,
                    message = "Download failed",
                    timestamp = DateTime.Now
                });
            }
        }

        [HttpGet("demo-settings")]
        public IActionResult GetDemoSettings()
        {
            return Ok(new FtpsSettings
            {
                Host = "demo.wftpserver.com",
                Port = 21,
                Username = "demo",
                Password = "demo",
                BasePath = "/",
                MovieDirectory = "download",
                SecurityMode = FtpsSecurityMode.None,
                ConnectionMode = FtpsConnectionMode.Passive,
                AcceptInvalidCertificates = true
            });
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new 
            { 
                service = "FTPS Integration Demo",
                status = "Running",
                version = "1.0.0",
                features = new[]
                {
                    "FTPS Connection Testing",
                    "Directory Browsing",
                    "File Download Simulation",
                    "SSL/TLS Support (None, Explicit, Implicit)",
                    "Active/Passive Mode Support",
                    "FluentValidation Integration",
                    "FluentFTP v48.0.2"
                },
                timestamp = DateTime.Now
            });
        }
    }

    public class FtpsListRequest
    {
        public FtpsSettings Settings { get; set; } = new();
        public string Path { get; set; } = "/";
    }

    public class FtpsDownloadRequest
    {
        public FtpsSettings Settings { get; set; } = new();
        public string RemotePath { get; set; } = "";
    }
}