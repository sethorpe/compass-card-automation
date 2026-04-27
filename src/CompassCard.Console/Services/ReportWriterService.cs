using CompassCard.Console.Models;

namespace CompassCard.Console.Services;

public class ReportWriterService
{
    public void WriteReport(List<CompassReloadRecord> records, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var totalLoaded   = records.Sum(r => r.ParsedAmount);
        var latestBalance = records.FirstOrDefault()?.ParsedBalance ?? 0m;
        var earliest      = records.LastOrDefault()?.DateTime  ?? "N/A";
        var latest        = records.FirstOrDefault()?.DateTime ?? "N/A";

        var byProduct = records
            .GroupBy(r => string.IsNullOrWhiteSpace(r.Product) ? "Unknown" : r.Product)
            .OrderByDescending(g => g.Sum(r => r.ParsedAmount));

        var lines = new List<string>
        {
            "╔══════════════════════════════════════════╗",
            "║       COMPASS CARD RELOAD REPORT         ║",
            "╚══════════════════════════════════════════╝",
            string.Empty,
            $"  Generated : {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
            $"  Period    : {earliest}  →  {latest}",
            $"  Reloads   : {records.Count}",
            $"  Total $   : {totalLoaded:C}",
            $"  Balance   : {latestBalance:C}  (as of most recent reload)",
            string.Empty,
            "── By Product ─────────────────────────────",
        };

        foreach (var group in byProduct)
            lines.Add($"  {group.Key,-30} {group.Count(),3} reload(s)   {group.Sum(r => r.ParsedAmount),8:C}");

        lines.Add(string.Empty);
        lines.Add("── All Reload Transactions ────────────────────────────────────");
        lines.Add($"  {"Date",-25} {"Product",-20} {"Amount",8}   {"Balance",10}");
        lines.Add($"  {new string('-', 68)}");

        foreach (var r in records)
            lines.Add($"  {r.DateTime,-25} {r.Product,-20} {r.ParsedAmount,8:C}   {r.ParsedBalance,10:C}");

        lines.Add(string.Empty);
        lines.Add("══════════════════════════════════════════");

        File.WriteAllLines(outputPath, lines);
        System.Console.WriteLine($"Report written: {outputPath}");
    }
}