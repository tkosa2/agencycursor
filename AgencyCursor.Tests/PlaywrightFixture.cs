using Microsoft.Playwright;
using System.Diagnostics;
using Xunit;

namespace AgencyCursor.Tests;

public class PlaywrightFixture : IAsyncLifetime
{
    public IBrowser Browser { get; private set; } = null!;
    private IPlaywright _playwright = null!;
    private Process? _webAppProcess;
    private const string WebAppUrl = "http://localhost:5084";
    private const int StartupTimeoutSeconds = 30;

    public async Task InitializeAsync()
    {
        // Start the web application
        await StartWebApplicationAsync();

        // Initialize Playwright and browser
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

    private async Task StartWebApplicationAsync()
    {
        // Get the path to the web app project
        // Walk up from current directory to find the solution folder
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        DirectoryInfo? solutionDir = null;
        
        while (currentDir != null)
        {
            if (File.Exists(Path.Combine(currentDir.FullName, "AgencyCursor.sln")))
            {
                solutionDir = currentDir;
                break;
            }
            currentDir = currentDir.Parent;
        }

        if (solutionDir == null)
        {
            throw new InvalidOperationException("Solution file (AgencyCursor.sln) not found");
        }

        var webAppDir = Path.Combine(solutionDir.FullName, "AgencyCursor.WebApp");

        if (!Directory.Exists(webAppDir))
        {
            throw new InvalidOperationException($"Web application directory not found at: {webAppDir}");
        }

        Console.WriteLine("Building web application...");
        var buildProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build --configuration Debug --no-restore",
            WorkingDirectory = webAppDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        if (buildProcess == null || !buildProcess.WaitForExit(180000))
        {
            buildProcess?.Kill(entireProcessTree: true);
            throw new TimeoutException("Web application build timed out after 180 seconds");
        }

        if (buildProcess.ExitCode != 0)
        {
            throw new InvalidOperationException("Failed to build web application");
        }
        Console.WriteLine("Web application built successfully");

        // Start the web application process
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --no-build",
            WorkingDirectory = webAppDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";

        _webAppProcess = Process.Start(startInfo);
        
        if (_webAppProcess == null)
        {
            throw new InvalidOperationException("Failed to start web application process");
        }

        // Wait for the application to be ready
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(StartupTimeoutSeconds)).Token;
        using var httpClient = new HttpClient();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await httpClient.GetAsync(WebAppUrl, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Web application started successfully at {WebAppUrl}");
                    await Task.Delay(1000); // Give it one more second to fully initialize
                    return;
                }
            }
            catch
            {
                // Server not ready yet, wait and retry
                await Task.Delay(500, cancellationToken);
            }
        }

        throw new TimeoutException($"Web application failed to start within {StartupTimeoutSeconds} seconds");
    }

    public async Task DisposeAsync()
    {
        // Close browser
        if (Browser != null)
        {
            await Browser.CloseAsync();
        }
        _playwright?.Dispose();

        // Stop the web application
        if (_webAppProcess != null && !_webAppProcess.HasExited)
        {
            _webAppProcess.Kill(entireProcessTree: true);
            _webAppProcess.WaitForExit(5000);
            _webAppProcess.Dispose();
            Console.WriteLine("Web application stopped");
        }
    }
}
