using Microsoft.Playwright;

namespace CompassCard.Console.Pages;

public class LoginPage : BasePage
{
    private ILocator EmailField    => Page.GetByLabel("Email address");
    private ILocator PasswordField => Page.GetByLabel("Password");
    private ILocator SignInButton  => Page.GetByRole(AriaRole.Button, new() { Name = "Sign in" });
    private ILocator ErrorMessage => Page.GetByRole(AriaRole.Status);

    public LoginPage(IPage page) : base(page) { }

    public async Task NavigateAsync(string baseUrl)
    {
        await Page.GotoAsync($"{baseUrl}/SignIn");
        await WaitForPageReadyAsync();
        await EmailField.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    public async Task LoginAsync(string username, string password)
    {
        await EmailField.FillAsync(username);
        await PasswordField.FillAsync(password);
        await SignInButton.ClickAsync();
        await Page.WaitForURLAsync("**/ManageCards");
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