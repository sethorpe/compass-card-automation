namespace CompassCard.Console.Config;

public class AppSettings
{
    // Set via environment variables: COMPASS_USERNAME, COMPASS_PASSWORD
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.compasscard.ca";
    public string CardNumber { get; set; } = string.Empty;
    public string DownloadPath { get; set; } = "downloads";
    public string ReportOutputPath { get; set; } = "reports/report.txt";
    public bool Headless { get; set; } = false;
}