using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using CompassCard.Console.Config;
using CompassCard.Console.Pages;

namespace CompassCard.Tests.Base;

public abstract class PlaywrightTestBase
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected AppSettings Settings { get; private set; } = null!;

    [OneTimeSetUp]
    public void LoadSettings()
    {
        Settings = new ConfigurationBuilder()
            .SetBasePath(FindRepoRoot())
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables(prefix: "COMPASS_")
            .AddEnvironmentVariables()
            .Build()
            .Get<AppSettings>() ?? new AppSettings();
    }

    [SetUp]
    public async Task SetUpBrowserAsync()
    {
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Settings.Headless
        });

        Context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            AcceptDownloads = true
        });

        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDownBrowserAsync()
    {
        await Context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    protected async Task NavigateAsAuthenticatedUserAsync()
    {
        var loginPage = new LoginPage(Page);
        await loginPage.NavigateAsync(Settings.BaseUrl);
        await loginPage.LoginAsync(Settings.Username, Settings.Password);
    }

    private static string FindRepoRoot()
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
}