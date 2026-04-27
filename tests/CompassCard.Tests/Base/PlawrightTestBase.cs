using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using CompassCard.Console.Config;

namespace CompassCard.Tests.Base;

public abstract class PlaywrightTestBase
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    protected AppSettings Settings { get; private set; } = null!;

    // Shared session path — same location as console app
    protected const string SessionPath = "auth/session.json";

    [OneTimeSetUp]
    public void LoadSettings()
    {
        Settings = new ConfigurationBuilder()
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

        var contextOptions = new BrowserNewContextOptions
        {
            AcceptDownloads = true
        };

        // Load saved session if available — same logic as console app
        if (File.Exists(SessionPath))
            contextOptions.StorageStatePath = SessionPath;

        Context = await _browser.NewContextAsync(contextOptions);
        Page = await Context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDownBrowserAsync()
    {
        await Context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    /// <summary>
    /// Navigates to ManageCards using session state.
    /// Call this at the start of any test that requires authentication.
    /// </summary>
    protected async Task NavigateAsAuthenticatedUserAsync()
    {
        await Page.GotoAsync($"{Settings.BaseUrl}/ManageCards");

        try
        {
            await Page.WaitForURLAsync("**/ManageCards",
                new PageWaitForURLOptions { Timeout = 10_000 });
        }
        catch (TimeoutException)
        {
            // Session didn't work
            throw new Exception(
                $"Session state failed - site redirected to {Page.Url} instead of ManageCards. " +
                "The session file may be expired or invalid in this environment. " +
                "Re-run SaveSession locally and update the COMPASS_SESSION secret.");
        }
        
    }
}