using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CompassCard.Console.Models;

namespace CompassCard.Console.Services;

public class CsvParserService
{
    public List<CompassReloadRecord> Parse(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            Encoding = System.Text.Encoding.UTF8
        };

        using var reader = new StreamReader(filePath, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<CompassReloadRecord>().ToList();
        System.Console.WriteLine($"Parsed:{records.Count} sales transaction records");
        return records;
    }    
}