using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using CompassCard.Console.Config;
using CompassCard.Console.Pages;
using CompassCard.Console.Services;

// Entry point - handles both normal run and session-save mode
if (args.Contains("--save-session"))
{
    await SaveSession.RunAsync();
    return 0;
}

// -- Configuration ----------------------------------------------
// Credentials MUST come from environment variables - never hardcoded
var config = new ConfigurationBuilder()
    .AddEnvironmentVariables(prefix: "COMPASS_") // COMPASS_USERNAME, COMPASS_PASSWORD
    .AddEnvironmentVariables() // Fallback: BaseUrl, Headless, etc
    .Build();

var settings = config.Get<AppSettings>() ?? new AppSettings();

if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
{
    Console.Error.WriteLine("COMPASS_USERNAME and COMPASS_PASSWORD environment variable must be set.");
    return 1;
}

Console.WriteLine($"Starting Compass Card automation [{DateTime.UtcNow:u}]");

// -- Playwright ---------------------------------------------------
using var playwright = await Playwright.CreateAsync();

await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = settings.Headless,
    SlowMo = settings.Headless ? 0 : 80, // Slow down for local visual debugging
    // Channel = "chrome",
});

const string sessionPath = "auth/session.json";

var contextOptions = new BrowserNewContextOptions
{
    AcceptDownloads = true
};

// Load saved session if it exists - skips login entirely
if (File.Exists(sessionPath))
{
    contextOptions.StorageStatePath = sessionPath;
    Console.WriteLine("Loaded saved session - skipping login");
}

var context = await browser.NewContextAsync(contextOptions);

var page = await context.NewPageAsync();

// -- Automation -----------------------------------------------------
try
{
    if (!File.Exists(sessionPath))
    {
        // Step 1: Login
        Console.WriteLine("-> Logging in...");
        var loginPage = new LoginPage(page);
        await loginPage.NavigateAsync(settings.BaseUrl);
        await loginPage.LoginAsync(settings.Username, settings.Password);
        Console.WriteLine("Logged in");    
    }
    else
    {
        // Session loaded - navigate directly to ManageCards
        await page.GotoAsync($"{settings.BaseUrl}/ManageCards");
        await page.WaitForURLAsync("**/ManageCards");
    }

    // Step 2: Select card and navigate to Card Usage
    Console.WriteLine("-> Navigating to Card Usage...");
    var manageCardsPage = new ManageCardsPage(page);
    await manageCardsPage.SelectCardAsync(settings.CardNumber);
    await manageCardsPage.NavigateToCardUsageAsync();
    Console.WriteLine("On Card Usage — Detailed View");

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
    var parser = new CsvParserService();
    var records = parser.Parse(csvPath);
    
    // Step 6: Write report
    Console.WriteLine("-> Writing report...");
    var writer = new ReportWriterService();
    writer.WriteReport(records, settings.ReportOutputPath);
    
    Console.WriteLine($"\nComplete! [{DateTime.UtcNow:u}]");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"\nAutomation failed: {ex.Message}");
    
    // Save a screenshot on failure
    var screenshotPath = $"reports/failure-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png";
    Directory.CreateDirectory("reports");
    await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
    Console.Error.WriteLine($"Screenshot saved: {screenshotPath}");

    return 1;
}