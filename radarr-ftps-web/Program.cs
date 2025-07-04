using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    
    // Register custom services
    builder.Services.AddScoped<RadarrFtpsWeb.Services.FtpsTestService>();
    
    // Configure logging
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    logger.Info("🚀 Application FTPS Web Test démarrée");
    
    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "❌ Erreur lors du démarrage de l'application");
    throw;
}
finally
{
    LogManager.Shutdown();
}