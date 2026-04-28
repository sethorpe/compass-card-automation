using System.Text.RegularExpressions;
using CompassCard.Console.Pages;
using CompassCard.Tests.Base;
using Microsoft.Playwright;

namespace CompassCard.Tests.Tests;

[TestFixture]
[Category("Login")]
public class LoginTests : PlaywrightTestBase
{
    [Test]
    [Description("Login page should load and display the email input field")]
    public async Task LoginPage_Loads_EmailFieldIsVisible()
    {
        var login = new LoginPage(Page);
        await login.NavigateAsync(Settings.BaseUrl);

        await Assertions.Expect(Page.GetByLabel("Email address")).ToBeVisibleAsync();
    }

    [Test]
    [Description("Invalid credentials should show an error message")]
    public async Task Login_WithInvalidCredentials_ShowsErrorMessage()
    {
        var login = new LoginPage(Page);
        await login.NavigateAsync(Settings.BaseUrl);
        await login.AttemptLoginAsync("invalid@email.com", "WrongPassword123!");

        Assert.That(await login.HasErrorMessageAsync(), Is.True,
            "Expected an error message for invalid credentials");
    }

    [Test]
    [Explicit("Excluded from automated runs — login page may present a CAPTCHA. " +
              "Run manually to verify valid credential flow.")]
    [Description("Valid credentials should redirect to ManageCards")]
    public async Task Login_WithValidCredentials_RedirectsToManageCards()
    {
        var login = new LoginPage(Page);
        await login.NavigateAsync(Settings.BaseUrl);
        await login.LoginAsync(Settings.Username, Settings.Password);

        await Assertions.Expect(Page).ToHaveURLAsync(new Regex("/ManageCards",
            RegexOptions.IgnoreCase));
    }
}