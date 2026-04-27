using Microsoft.Playwright;

namespace CompassCard.Console.Pages;

public class CardUsagePage : BasePage
{
    private ILocator StartDateInput => 
        Page.Locator("#Content_ManageCard_compCardHistory_txtStartDate");

    private ILocator EndDateInput => 
        Page.Locator("#Content_ManageCard_compCardHistory_txtEndDate");

    private ILocator ShowDateHistoryDropdown => 
        Page.Locator("#Content_ManageCard_compCardHistory_lstDate");

    private ILocator CardUsageLabel => 
        Page.Locator("label[for='Content_ManageCard_compCardHistory_CheckBoxListDataOptionUsage']");

    private ILocator PaymentsLabel => 
        Page.Locator("label[for='Content_ManageCard_compCardHistory_CheckBoxListDataOptionPayment']");

    private ILocator CardUsageCheckbox => 
        Page.Locator("#Content_ManageCard_compCardHistory_CheckBoxListDataOptionUsage");

    private ILocator PaymentsCheckbox => 
        Page.Locator("#Content_ManageCard_compCardHistory_CheckBoxListDataOptionPayment");

    private ILocator DownloadCsvLink => 
        Page.GetByRole(AriaRole.Link, new() { Name = "Download CSV" });

    public CardUsagePage(IPage page) : base(page) { }

    /// <summary>
    /// Sets the date range to the previous calendar month.
    /// Uses JavaScript to set values because WebForms date inputs
    /// don't respond to standard FillAsync.
    /// </summary>
    public async Task SetPreviousMonthDateRangeAsync()
    {
        var (startDate, endDate) = GetPreviousMonthRange();

        await ShowDateHistoryDropdown.SelectOptionAsync(new[] { "custom" });
        await WaitForPageReadyAsync();

        await StartDateInput.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        await SetDateViaJavaScriptAsync(
            "Content_ManageCard_compCardHistory_txtStartDate",
            startDate.ToString("MMM-dd-yyyy"));
        await WaitForPageReadyAsync();

        await SetDateViaJavaScriptAsync(
            "Content_ManageCard_compCardHistory_txtEndDate",
            endDate.ToString("MMM-dd-yyyy"));
        await WaitForPageReadyAsync();
    }

    /// <summary>
    /// Unchecks "Card Usage" and ensures "Payments" is checked.
    /// Clicks the label rather than the checkbox directly — required
    /// for WebForms controls to trigger the postback correctly.
    /// </summary>
    public async Task SelectPaymentsOnlyAsync()
    {
        if (await CardUsageCheckbox.IsCheckedAsync())
        {
            await CardUsageLabel.ClickAsync();
            await WaitForPageReadyAsync();
            await Task.Delay(1000);   // Allow postback to settle
        }

        if (!await PaymentsCheckbox.IsCheckedAsync())
        {
            await PaymentsLabel.ClickAsync();
            await WaitForPageReadyAsync();
            await Task.Delay(1000);   // Allow postback to settle
        }
    }

    public async Task<string> DownloadCsvAsync(string downloadPath)
    {
        Directory.CreateDirectory(downloadPath);

        var downloadTask = Page.WaitForDownloadAsync();
        await DownloadCsvLink.ClickAsync();
        var download = await downloadTask;

        var filePath = Path.Combine(downloadPath, download.SuggestedFilename);
        await download.SaveAsAsync(filePath);

        System.Console.WriteLine($"Downloaded: {filePath}");
        return filePath;
    }

    private static (DateTime StartDate, DateTime EndDate) GetPreviousMonthRange()
    {
        var today = DateTime.Today;
        var firstOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
        var startDate = firstOfCurrentMonth.AddMonths(-1);
        var endDate = firstOfCurrentMonth.AddDays(-1);
        return (startDate, endDate);
    }

    private async Task SetDateViaJavaScriptAsync(string elementId, string value)
    {
        await Page.EvaluateAsync(@"(args) => {
            const el = document.getElementById(args.id);
            el.value = args.value;
            el.dispatchEvent(new Event('change'));
        }", new { id = elementId, value });
    }
}