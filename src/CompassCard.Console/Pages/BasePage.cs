using Microsoft.Playwright;

namespace CompassCard.Console.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected const int DefaultTimeoutMs = 15_000;

    protected BasePage(IPage page)
    {
        Page = page;
    }

    /// <summary>
    /// Waits for WebForms postbacks and page transitions to complete.
    /// </summary>
    protected async Task WaitForPageReadyAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    protected async Task WaitForVisibleAsync(string selector, int timeoutMs = DefaultTimeoutMs)
    {
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
    }
}