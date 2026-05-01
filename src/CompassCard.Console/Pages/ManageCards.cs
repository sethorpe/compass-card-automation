using Microsoft.Playwright;
using Serilog;

namespace CompassCard.Console.Pages;

public class ManageCardsPage : BasePage
{
    private ILocator ViewCardUsageLink =>
        Page.GetByRole(AriaRole.Link, new() { Name = " View card usage" });

    private ILocator DetailedViewTab =>
        Page.GetByRole(AriaRole.Link, new() { Name = " Detailed View" });

    private ILocator GetCardLocator(string cardNumber) =>
        Page.GetByRole(AriaRole.Link, new() { Name = cardNumber });

    public ManageCardsPage(IPage page, ILogger? logger = null) : base(page, logger) { }

    public async Task SelectCardAsync(string cardNumber)
    {
        Logger?.Debug("Selecting card: {CardNumber}", cardNumber);
        await GetCardLocator(cardNumber).ClickAsync();
        await WaitForPageReadyAsync();
        Logger?.Information("Card selected");
    }

    public async Task NavigateToCardUsageAsync()
    {
        Logger?.Debug("Clicking View Card Usage link...");
        await ViewCardUsageLink.ClickAsync();
        await WaitForPageReadyAsync();

        Logger?.Debug("Clicking Detailed View tab...");
        await DetailedViewTab.ClickAsync();
        await WaitForPageReadyAsync();
        Logger?.Information("Navigated to Card Usage - Detailed View");
    }
}
