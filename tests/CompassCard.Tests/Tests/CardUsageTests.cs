using System.Text.RegularExpressions;
using CompassCard.Console.Pages;
using CompassCard.Console.Services;
using CompassCard.Tests.Base;
using Microsoft.Playwright;

namespace CompassCard.Tests.Tests;

[TestFixture]
[Category("CardUsage")]
[Explicit("Requires valid session state. Run locally only - session cannot be replayed from CI.")]
public class CardUsageTests : PlaywrightTestBase
{
    private const string TestDownloadPath = "test-downloads";

    [Test]
    [Description("Should navigate to Card Usage — Detailed View after selecting a card")]
    public async Task CardUsage_Navigation_ReachesDetailedView()
    {
        await NavigateAsAuthenticatedUserAsync();

        var manageCardsPage = new ManageCardsPage(Page);
        await manageCardsPage.SelectCardAsync(Settings.CardNumber);
        await manageCardsPage.NavigateToCardUsageAsync();

        // CardUsage is part of ManageCards - URL doesn't change.
        // Assert the Detailed View tab content is visible instead.
        await Assertions.Expect(
            Page.GetByRole(AriaRole.Link, new() { Name = "Detailed View" })
        ).ToBeVisibleAsync();
    }

    [Test]
    [Description("CSV download with Payments filter should produce a non-empty .csv file")]
    public async Task Download_WithPaymentsFilter_ProducesNonEmptyCsvFile()
    {
        await NavigateAsAuthenticatedUserAsync();

        var manageCardsPage = new ManageCardsPage(Page);
        await manageCardsPage.SelectCardAsync(Settings.CardNumber);
        await manageCardsPage.NavigateToCardUsageAsync();

        var cardUsagePage = new CardUsagePage(Page);
        await cardUsagePage.SetPreviousMonthDateRangeAsync();
        await cardUsagePage.SelectPaymentsOnlyAsync();

        var filePath = await cardUsagePage.DownloadCsvAsync(TestDownloadPath);

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(filePath), Is.True,
                "Downloaded file must exist");
            Assert.That(new FileInfo(filePath).Length, Is.GreaterThan(0),
                "Downloaded file must not be empty");
            Assert.That(Path.GetExtension(filePath), Is.EqualTo(".csv").IgnoreCase,
                "File extension must be .csv");
        });
    }

    [Test]
    [Description("Parsed CSV should contain at least one reload record")]
    public async Task Download_ParsedCsv_ContainsAtLeastOneReloadRecord()
    {
        await NavigateAsAuthenticatedUserAsync();

        var manageCardsPage = new ManageCardsPage(Page);
        await manageCardsPage.SelectCardAsync(Settings.CardNumber);
        await manageCardsPage.NavigateToCardUsageAsync();

        var cardUsagePage = new CardUsagePage(Page);
        await cardUsagePage.SetPreviousMonthDateRangeAsync();
        await cardUsagePage.SelectPaymentsOnlyAsync();

        var filePath = await cardUsagePage.DownloadCsvAsync(TestDownloadPath);
        var records = new CsvParserService().Parse(filePath);

        Assert.That(records, Is.Not.Empty,
            "Expected at least one reload record in the downloaded CSV");
    }

    [TearDown]
    public void CleanupTestDownloads()
    {
        if (Directory.Exists(TestDownloadPath))
            Directory.Delete(TestDownloadPath, recursive: true);
    }
}