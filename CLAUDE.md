# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build --configuration Debug
dotnet build --configuration Release

# Run console automation
dotnet run --project src/CompassCard.Console

# Run unit tests only (no browser, safe for CI)
dotnet test tests/CompassCard.Tests --filter "Category=Unit"

# Run all tests including E2E (requires credentials and headed browser)
dotnet test tests/CompassCard.Tests

# Run a single test by name
dotnet test tests/CompassCard.Tests --filter "Name=Login_WithValidCredentials_RedirectsToManageCards"
```

## Architecture

A .NET 8 console app that uses Playwright (Firefox) to log into compasscard.ca, download the previous month's reload transactions as a CSV, parse them with CsvHelper, and write a formatted report. A separate NUnit test project shares the page objects via `ProjectReference`.

**Automation flow** (`Program.cs`):
1. Load config → `LoginPage.LoginAsync` → `ManageCardsPage.SelectCardAsync` / `NavigateToCardUsageAsync` → `CardUsagePage.SetPreviousMonthDateRangeAsync` / `SelectPaymentsOnlyAsync` / `DownloadCsvAsync` → `CsvParserService.Parse` → `ReportWriterService.WriteReport`

**Page Object Model** — all pages extend `BasePage` which holds the `IPage` and optional `ILogger?`. Pages accept `ILogger?` as an optional constructor parameter so they work in both the console app (with Serilog) and test fixtures (without).

**Config loading** (`Program.cs` and `PlaywrightTestBase.cs`) — uses `FindRepoRoot()` to walk up from `Directory.GetCurrentDirectory()` until it finds `.git`, then loads `appsettings.Local.json` from there. This ensures the file is found regardless of whether the working directory is the repo root, project directory, or bin directory. Environment variables override the file (CI uses GitHub Secrets).

**Logging** — Serilog with console + file sinks. Log file written to `logs/compass_automation_{timestamp}.log` on every run. Each page class is given a contextualized logger at instantiation in `Program.cs`:
```csharp
new LoginPage(page, Log.ForContext("SourceContext", "compass_automation.login"))
```
Output template: `{Timestamp:yyyy-MM-dd HH:mm:ss} - {SourceContext} - {Level:u3} - {Message:lj}`

**Login error handling** — `LoginPage.LoginAsync` waits for `NetworkIdle` after clicking Sign In, then immediately inspects the result rather than waiting for a URL pattern to match. Throws descriptive exceptions for a 500 error page or any unexpected redirect, avoiding a 30-second timeout hang.

**Test categories**:
- `Unit` — CSV parsing only, no browser, runs in CI
- `Login` — browser-based login page tests, local only
- `CardUsage` — full E2E, local only

The valid-credentials login test is marked `[Explicit]` and must be run by name — compasscard.ca may present a CAPTCHA.

## Local credentials

Copy `appsettings.example.json` → `appsettings.Local.json` at the repo root and fill in `Username`, `Password`, `CardNumber`. The file is gitignored.

## Commit conventions

Conventional Commits strictly. Allowed prefixes: `feat`, `fix`, `chore`, `refactor`, `test`, `docs`, `ci`. Present tense, subject under 72 characters. No mentions of AI tools, no `Co-authored-by` lines.

## Git workflow

branch → commit → `git push origin <branch>` → `gh pr create` → `gh pr merge` → `git checkout main` → `git pull`

## Known issues

compasscard.ca applies bot-detection at the account level. Heavy local testing can trigger CAPTCHA or 500 errors on login that affect all environments (including CI) until the threshold resets. The automation is otherwise correct — wait for the block to lift and trigger a manual CI run to confirm.
