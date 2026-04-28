# Compass Card Reload Automation

An SDET portfolio project that automates the monthly download and reporting of reload transactions from [compasscard.ca](https://www.compasscard.ca) using C# and Playwright.

---

## Overview

This project demonstrates end-to-end browser automation, the Page Object Model design pattern, and CI/CD integration via GitHub Actions. It runs on the 1st of every month, downloads a CSV of reload transactions from the previous month, parses the data, and produces a plain-text report.

---

## Architecture

```
compass-card-automation/
 ├── src/
 │   └── CompassCard.Console/        # Automation runner (console app)
 │       ├── Config/                 # Typed configuration
 │       ├── Models/                 # CSV record model
 │       ├── Pages/                  # Page Object Model
 │       │   ├── BasePage.cs
 │       │   ├── LoginPage.cs
 │       │   ├── ManageCardsPage.cs
 │       │   └── CardUsagePage.cs
 │       └── Services/               # CSV parsing and report writing
 ├── tests/
 │   └── CompassCard.Tests/          # NUnit test suite
 │       ├── Base/                   # Shared browser setup
 │       └── Tests/                  # Login, CardUsage, and Unit tests
 └── .github/
     └── workflows/
         └── automation.yml          # GitHub Actions cron workflow
```

---

## Tech Stack

| | |
|---|---|
| Language | C# / .NET 8 |
| Browser Automation | Microsoft Playwright |
| Test Framework | NUnit |
| CSV Parsing | CsvHelper |
| CI/CD | GitHub Actions |
| Browser | Firefox |

---

## How It Works

1. **Login** — authenticates with Compass Card using credentials stored as environment variables
2. **Navigate** — selects the target card and opens the Card Usage — Detailed View
3. **Filter** — sets the date range to the previous calendar month, selects Payments (reloads) only
4. **Download** — downloads the filtered transaction history as a CSV
5. **Parse** — maps CSV rows to typed `CompassReloadRecord` models using CsvHelper
6. **Report** — writes a summary report to `reports/report.txt`

---

## Local Setup

### Prerequisites
- .NET 8 SDK
- [Playwright CLI](https://playwright.dev/dotnet/docs/intro)

```bash
dotnet tool install --global Microsoft.Playwright.CLI
playwright install firefox
```

### Environment Variables

| Variable | Description |
|---|---|
| `COMPASS_USERNAME` | compasscard.ca login email |
| `COMPASS_PASSWORD` | compasscard.ca password |
| `COMPASS_CARDNUMBER` | Card number as it appears on the site |

Set these in Rider via **Run → Edit Configurations → Environment Variables**.

### Running the Console App

```bash
dotnet run --project src/CompassCard.Console
```

> Set `Headless = false` in `AppSettings.cs` during development to watch the browser run.

### Running the Tests

**Unit tests only (no browser required):**
```bash
dotnet test tests/CompassCard.Tests --filter "Category=Unit"
```

**All tests (requires credentials and a headed browser locally):**
```bash
dotnet test tests/CompassCard.Tests
```

> E2E tests require the working directory to be set to the repo root in Rider's run configuration so credentials resolve correctly.

---

## CI/CD

The GitHub Actions workflow runs automatically on the **1st of every month at midnight UTC**. It can also be triggered manually from the **Actions** tab.

**Workflow steps:**
1. Build the solution
2. Install Playwright + Firefox
3. Run unit tests
4. Run the console automation
5. Upload `report.txt` as a downloadable artifact (retained for 60 days)

### Required GitHub Secrets

| Secret | Description |
|---|---|
| `COMPASS_USERNAME` | compasscard.ca login email |
| `COMPASS_PASSWORD` | compasscard.ca password |
| `COMPASS_CARDNUMBER` | Target card number |

---

## Test Strategy

| Category | Type | Runs in CI |
|---|---|---|
| `Unit` | CSV parsing logic — no browser | Yes |
| `Login` | Login page load and error states | Locally only |
| `CardUsage` | Full E2E — navigate, filter, download | Locally only |

E2E tests are excluded from CI because compasscard.ca ties sessions to the originating IP, making browser-based login unreliable from GitHub Actions runners. The console app handles this via a direct login on each scheduled run.

---

## Output

`reports/report.txt` — generated on each run:

```
╔══════════════════════════════════════════╗
║       COMPASS CARD RELOAD REPORT         ║
╚══════════════════════════════════════════╝

  Generated : 2026-04-01 00:02:11 UTC
  Period    : Mar-01-2026 ...  →  Mar-31-2026 ...
  Reloads   : 6
  Total $   : $120.00
  Balance   : $21.90  (as of most recent reload)

── By Product ─────────────────────────────
  Stored Value                     6 reload(s)   $120.00
...
```
