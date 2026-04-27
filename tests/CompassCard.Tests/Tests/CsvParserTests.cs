using CompassCard.Console.Services;
using CompassCard.Tests.Base;

namespace CompassCard.Tests.Tests;

[TestFixture]
[Category("Unit")]
public class CsvParserTests
{
    private string _tempFile = string.Empty;

    [SetUp]
    public void CreateTempFile() =>
        _tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".csv");

    [Test]
    [Description("Parser should map all columns from a valid Compass Card CSV")]
    public void Parse_ValidCompassCsv_ReturnsAllRecords()
    {
        // Headers and format match the real Compass Card export exactly
        File.WriteAllText(_tempFile, """
            DateTime,Transaction,Product,LineItem,Amount,BalanceDetails,JourneyId,LocationDisplay,TransactonTime,OrderDate,Payment,OrderNumber,AuthCode,Total
            Mar-31-2026 07:58 AM,AutoLoaded,Stored Value,,$20.00,$21.90,,"AutoLoaded
            Stored Value",07:58 AM,,,,,
            Mar-24-2026 04:49 PM,AutoLoaded,Stored Value,,$20.00,$24.30,,"AutoLoaded
            Stored Value",04:49 PM,,,,,
            """);

        var records = new CsvParserService().Parse(_tempFile);

        Assert.That(records.Count, Is.EqualTo(2));
        Assert.That(records[0].Transaction, Is.EqualTo("AutoLoaded"));
        Assert.That(records[0].Product, Is.EqualTo("Stored Value"));
        Assert.That(records[0].ParsedAmount, Is.EqualTo(20.00m));
        Assert.That(records[0].ParsedBalance, Is.EqualTo(21.90m));
    }

    [Test]
    [Description("ParsedAmount should correctly strip the dollar sign and parse as decimal")]
    public void ParsedAmount_WithDollarSign_ParsesCorrectly()
    {
        File.WriteAllText(_tempFile, """
            DateTime,Transaction,Product,LineItem,Amount,BalanceDetails,JourneyId,LocationDisplay,TransactonTime,OrderDate,Payment,OrderNumber,AuthCode,Total
            Mar-31-2026 07:58 AM,AutoLoaded,Stored Value,,$20.00,$21.90,,"AutoLoaded
            Stored Value",07:58 AM,,,,,
            """);

        var records = new CsvParserService().Parse(_tempFile);

        Assert.That(records[0].ParsedAmount, Is.EqualTo(20.00m));
        Assert.That(records[0].ParsedBalance, Is.EqualTo(21.90m));
    }

    [Test]
    [Description("Parser should return an empty list for a header-only CSV")]
    public void Parse_HeaderOnlyCsv_ReturnsEmptyList()
    {
        File.WriteAllText(_tempFile,
            "DateTime,Transaction,Product,LineItem,Amount,BalanceDetails,JourneyId,LocationDisplay,TransactonTime,OrderDate,Payment,OrderNumber,AuthCode,Total\n");

        Assert.That(new CsvParserService().Parse(_tempFile), Is.Empty);
    }

    [Test]
    [Description("ParsedAmount should return 0 for unparseable or empty amount")]
    public void ParsedAmount_WithEmptyAmount_ReturnsZero()
    {
        File.WriteAllText(_tempFile, """
            DateTime,Transaction,Product,LineItem,Amount,BalanceDetails,JourneyId,LocationDisplay,TransactonTime,OrderDate,Payment,OrderNumber,AuthCode,Total
            Mar-31-2026 07:58 AM,AutoLoaded,Stored Value,,,$21.90,,"AutoLoaded
            Stored Value",07:58 AM,,,,,
            """);

        var records = new CsvParserService().Parse(_tempFile);

        Assert.That(records[0].ParsedAmount, Is.EqualTo(0m));
    }

    [TearDown]
    public void Cleanup()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }
}