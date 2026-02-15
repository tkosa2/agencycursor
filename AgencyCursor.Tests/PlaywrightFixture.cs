using Microsoft.Playwright;
using Xunit;

namespace AgencyCursor.Tests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IBrowser Browser { get; private set; } = null!;
    private IPlaywright _playwright = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        
        // Try to launch browser, install if needed
        try
        {
            Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
        catch
        {
            // If browser is not installed, install it and try again
            Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
            Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
    }

    public async Task DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }
        _playwright?.Dispose();
    }
}
