using Microsoft.Playwright;
using Serilog;

namespace CompassCard.Console.Pages;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly ILogger? Logger;
    protected const int DefaultTimeoutMs = 15_000;

    protected BasePage(IPage page, ILogger? logger = null)
    {
        Page = page;
        Logger = logger;
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
