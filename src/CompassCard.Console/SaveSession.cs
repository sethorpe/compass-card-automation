using Microsoft.Playwright;

/// <summary>
/// One-time utility to manually log in and persist the session.
/// Run this whenever the session expires.
/// Usage: dotnet run --project src/CompassCard.Console -- --save-session
/// </summary>
public static class SaveSession
{
    public static async Task RunAsync()
    {
        const string sessionPath = "auth/session.json";
        Directory.CreateDirectory("auth");

        Console.WriteLine("Opening browser — log in manually, then close the Playwright Inspector to save your session...");

        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            AcceptDownloads = true
        });

        var page = await context.NewPageAsync();

        await page.GotoAsync("https://www.compasscard.ca/SignIn");

        // Execution pauses here — Playwright Inspector opens.
        // Log in manually (solve CAPTCHA, enter credentials), then
        // click the Resume button in the Inspector to continue.
        await page.PauseAsync();

        // At this point you should be on /ManageCards
        await context.StorageStateAsync(new BrowserContextStorageStateOptions
        {
            Path = sessionPath
        });

        Console.WriteLine($"Session saved to {sessionPath}");
        Console.WriteLine("Add auth/session.json to .gitignore — never commit this file.");
    }
}