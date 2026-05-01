using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
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

Console.WriteLine($"Starting Compass Card automation [{DateTime.UtcNow:u}]");

// -- Playwright ---------------------------------------------------
using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = settings.Headless,
    SlowMo = settings.Headless ? 0 : 80
});

var context = await browser.NewContextAsync(new BrowserNewContextOptions
{
    AcceptDownloads = true
});

var page = await context.NewPageAsync();

// -- Automation -----------------------------------------------------
try
{
    // Step 1: Login
    Console.WriteLine("-> Logging in...");
    var loginPage = new LoginPage(page);
    await loginPage.NavigateAsync(settings.BaseUrl);
    await loginPage.LoginAsync(settings.Username, settings.Password);
    Console.WriteLine("Logged in");

    // Step 2: Select card and navigate to Card Usage
    Console.WriteLine("-> Navigating to Card Usage...");
    var manageCardsPage = new ManageCardsPage(page);
    await manageCardsPage.SelectCardAsync(settings.CardNumber);
    await manageCardsPage.NavigateToCardUsageAsync();
    Console.WriteLine("On Card Usage - Detailed View");

    // Step 3: Set date range and filter to Payments (reloads) only
    Console.WriteLine("-> Configuring filters...");
    var cardUsagePage = new CardUsagePage(page);
    await cardUsagePage.SetPreviousMonthDateRangeAsync();
    await cardUsagePage.SelectPaymentsOnlyAsync();
    Console.WriteLine("Filters applied: previous month, payments only");

    // Step 4: Download
    Console.WriteLine("-> Downloading CSV...");
    var csvPath = await cardUsagePage.DownloadCsvAsync(settings.DownloadPath);

    // Step 5: Parse
    Console.WriteLine("-> Parsing CSV...");
    var records = new CsvParserService().Parse(csvPath);

    // Step 6: Write report
    Console.WriteLine("-> Writing report...");
    new ReportWriterService().WriteReport(records, settings.ReportOutputPath);

    Console.WriteLine($"\nComplete! [{DateTime.UtcNow:u}]");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"\nAutomation failed: {ex.Message}");

    var screenshotPath = $"reports/failure-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png";
    Directory.CreateDirectory("reports");
    await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
    Console.Error.WriteLine($"Screenshot saved: {screenshotPath}");

    return 1;
}