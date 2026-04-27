using Microsoft.Playwright;

namespace CompassCard.Console.Pages;

public class ManageCardsPage : BasePage
{
    private ILocator ViewCardUsageLink => 
        Page.GetByRole(AriaRole.Link, new() { Name = " View card usage" });

    private ILocator DetailedViewTab => 
        Page.GetByRole(AriaRole.Link, new() { Name = " Detailed View" });

    private ILocator GetCardLocator(string cardNumber) =>
        Page.GetByRole(AriaRole.Link, new() { Name = cardNumber });

    public ManageCardsPage(IPage page) : base(page) { }

    public async Task SelectCardAsync(string cardNumber)
    {
        await GetCardLocator(cardNumber).ClickAsync();
        await WaitForPageReadyAsync();
    }

    public async Task NavigateToCardUsageAsync()
    {
        await ViewCardUsageLink.ClickAsync();
        await WaitForPageReadyAsync();

        await DetailedViewTab.ClickAsync();
        await WaitForPageReadyAsync();
    }
}