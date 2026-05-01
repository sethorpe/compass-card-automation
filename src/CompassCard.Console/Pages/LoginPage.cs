using Microsoft.Playwright;
using Serilog;

namespace CompassCard.Console.Pages;

public class LoginPage : BasePage
{
    private ILocator EmailField    => Page.GetByLabel("Email address");
    private ILocator PasswordField => Page.GetByLabel("Password");
    private ILocator SignInButton  => Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" });
    private ILocator ErrorMessage  => Page.GetByRole(AriaRole.Status);

    public LoginPage(IPage page, ILogger? logger = null) : base(page, logger) { }

    public async Task NavigateAsync(string baseUrl)
    {
        Logger?.Debug("Navigating to {Url}...", $"{baseUrl}/SignIn");
        await Page.GotoAsync($"{baseUrl}/SignIn");
        await WaitForPageReadyAsync();
        await EmailField.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        Logger?.Information("Login page loaded");
    }

    public async Task LoginAsync(string username, string password)
    {
        Logger?.Debug("Filling credentials for: {Username}", username);
        await EmailField.FillAsync(username);
        await PasswordField.FillAsync(password);
        Logger?.Debug("Login form submitted, waiting for redirect...");
        await SignInButton.ClickAsync();
        await Page.WaitForURLAsync("**/ManageCards");
        Logger?.Information("Login successful, redirected to ManageCards");
    }

    public async Task AttemptLoginAsync(string username, string password)
    {
        await EmailField.FillAsync(username);
        await PasswordField.FillAsync(password);
        await SignInButton.ClickAsync();
    }

    public async Task<bool> HasErrorMessageAsync()
    {
        try
        {
            await ErrorMessage.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 3_000
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}