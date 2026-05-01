using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Serilog;
using CompassCard.Console.Config;
using CompassCard.Console.Pages;
using CompassCard.Console.Services;

// -- Configuration ----------------------------------------------
var config = new ConfigurationBuilder()
    .SetBasePath(FindRepoRoot())
    .AddJsonFile("appsettings.Local.json", optional: true)
    .AddEnvironmentVariables(prefix: "COMPASS_")
    .AddEnvironmentVariables()
    .Build();

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return Directory.GetCurrentDirectory();
}

var settings = config.Get<AppSettings>() ?? new AppSettings();

if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
{
    Console.Error.WriteLine("COMPASS_USERNAME and COMPASS_PASSWORD environment variables must be set.");
    return 1;
}

// -- Logging -------------------------------------------------------
var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
var logPath = Path.Combine("logs", $"compass_automation_{timestamp}.log");
Directory.CreateDirectory("logs");

const string outputTemplate =
    "{Timestamp:yyyy-MM-dd HH:mm:ss} - {SourceContext} - {Level:u3} - {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: outputTemplate)
    .WriteTo.File(logPath, outputTemplate: outputTemplate)
    .CreateLogger();

var log = Log.ForContext("SourceContext", "compass_automation.main");
log.Information("Logging to file: {LogPath}", logPath);

log.Information("============================================================");
log.Information("Compass Card Automation Started");
log.Information("============================================================");

// -- Playwright ---------------------------------------------------
log.Information("Launching browser...");
using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = settings.Headless,
    SlowMo = settings.Headless ? 0 : 80
});
log.Debug("Browser launched: firefox");

var context = await browser.NewContextAsync(new BrowserNewContextOptions
{
    AcceptDownloads = true
});

var page = await context.NewPageAsync();
log.Debug("Browser context and page created");

// -- Automation -----------------------------------------------------
try
{
    // Step 1: Login
    log.Information("Logging in...");
    var loginPage = new LoginPage(page, Log.ForContext("SourceContext", "compass_automation.login"));
    await loginPage.NavigateAsync(settings.BaseUrl);
    await loginPage.LoginAsync(settings.Username, settings.Password);
    log.Information("Login successful");

    // Step 2: Select card and navigate to Card Usage
    log.Information("Navigating to Card Usage...");
    var manageCardsPage = new ManageCardsPage(page, Log.ForContext("SourceContext", "compass_automation.manage_cards"));
    await manageCardsPage.SelectCardAsync(settings.CardNumber);
    await manageCardsPage.NavigateToCardUsageAsync();
    log.Information("On Card Usage - Detailed View");

    // Step 3: Set date range and filter to Payments (reloads) only
    log.Information("Configuring filters...");
    var cardUsagePage = new CardUsagePage(page, Log.ForContext("SourceContext", "compass_automation.card_usage"));
    await cardUsagePage.SetPreviousMonthDateRangeAsync();
    await cardUsagePage.SelectPaymentsOnlyAsync();
    log.Information("Filters applied: previous month, payments only");

    // Step 4: Download
    log.Information("Downloading CSV...");
    var csvPath = await cardUsagePage.DownloadCsvAsync(settings.DownloadPath);

    // Step 5: Parse
    log.Debug("Parsing CSV...");
    var records = new CsvParserService().Parse(csvPath);
    log.Debug("Parsed {Count} reload records", records.Count);

    // Step 6: Write report
    log.Information("Writing report...");
    new ReportWriterService().WriteReport(records, settings.ReportOutputPath);
    log.Debug("Report written: {Path}", settings.ReportOutputPath);

    log.Information("============================================================");
    log.Information("Automation completed successfully");
    log.Information("============================================================");
    return 0;
}
catch (Exception ex)
{
    log.Error(ex, "Automation failed: {Message}", ex.Message);

    var screenshotPath = $"reports/failure-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png";
    Directory.CreateDirectory("reports");
    await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
    log.Error("Screenshot saved: {ScreenshotPath}", screenshotPath);

    return 1;
}
finally
{
    Log.CloseAndFlush();
}
