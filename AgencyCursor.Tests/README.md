# AgencyCursor.Tests

Playwright tests for the AgencyCursor application.

## Setup

1. Install Playwright browsers:
   ```bash
   dotnet build
   ```
   This will automatically install the required browsers.

   Or manually:
   ```bash
   pwsh -Command "playwright install chromium"
   ```

2. Make sure the application is running on `http://localhost:5084` (or update the `BaseUrl` in `RequestTests.cs`)

## Running Tests

Run all tests:
```bash
dotnet test
```

Run a specific test:
```bash
dotnet test --filter "CreateNewRequest_ShouldSubmitSuccessfully"
```

## Test Structure

- `PlaywrightFixture.cs` - Shared fixture that initializes the Playwright browser
- `RequestTests.cs` - Tests for creating new requests (both In-Person and Virtual)

## Configuration

The tests assume the application is running on `http://localhost:5084`. To change this, update the `BaseUrl` constant in `RequestTests.cs`.
