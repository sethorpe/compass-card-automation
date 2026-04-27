using CsvHelper.Configuration.Attributes;

namespace CompassCard.Console.Models;

public class CompassReloadRecord
{
    [Name("DateTime")]
    public string DateTime { get; set; } = string.Empty;

    [Name("Transaction")]
    public string Transaction { get; set; } = string.Empty;

    [Name("Product")]
    public string Product { get; set; } = string.Empty;

    [Name("LineItem")]
    public string LineItem { get; set; } = string.Empty;

    [Name("Amount")]
    public string Amount { get; set; } = string.Empty;

    [Name("BalanceDetails")]
    public string BalanceDetails { get; set; } = string.Empty;

    [Name("JourneyId")]
    public string JourneyId { get; set; } = string.Empty;

    [Name("LocationDisplay")]
    public string LocationDisplay { get; set; } = string.Empty;

    // Typo is intentional — matches the actual CSV header exactly
    [Name("TransactonTime")]
    public string TransactonTime { get; set; } = string.Empty;

    [Name("OrderDate")]
    public string OrderDate { get; set; } = string.Empty;

    [Name("Payment")]
    public string Payment { get; set; } = string.Empty;

    [Name("OrderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [Name("AuthCode")]
    public string AuthCode { get; set; } = string.Empty;

    [Name("Total")]
    public string Total { get; set; } = string.Empty;

    // Helpers — strip "$" and parse for calculations
    public decimal ParsedAmount =>
        decimal.TryParse(Amount.Replace("$", "").Trim(), out var val) ? val : 0m;

    public decimal ParsedBalance =>
        decimal.TryParse(BalanceDetails.Replace("$", "").Trim(), out var val) ? val : 0m;
}