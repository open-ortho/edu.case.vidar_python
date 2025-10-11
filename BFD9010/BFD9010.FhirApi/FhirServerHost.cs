using BFD9010.Scanner;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BFD9010.FhirApi;

/// <summary>
/// Manages the lifecycle of the FHIR API server for both CLI and GUI applications
/// </summary>
public class FhirServerHost : IDisposable
{
    private IHost? _webHost;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _serverTask;
    private readonly string? _configPath;

    public FhirServerHost(string? configPath = null)
    {
        _configPath = configPath;
    }

    /// <summary>
    /// Start the FHIR API server
    /// </summary>
    /// <returns>True if scanner was initialized successfully, false otherwise</returns>
    public async Task<bool> StartAsync()
    {
        // Load configuration to get CORS and URL settings
        var config = ScanConfig.Load(_configPath);

        // Create web application using shared configuration
        var builder = WebApplication.CreateBuilder();

        // Configure logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Configure services using shared configuration with config
        FhirServerConfiguration.ConfigureServices(builder, config);

        // Build the app
        var app = builder.Build();

        // Configure FHIR endpoints using shared configuration
        FhirServerConfiguration.ConfigureEndpoints(app);

        // Create cancellation token for controlling server lifetime
        _cancellationTokenSource = new CancellationTokenSource();

        // Start the web server in background
        _serverTask = Task.Run(async () =>
        {
            try
            {
                await app.RunAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping the server
            }
        }, _cancellationTokenSource.Token);

        // Store the host
        _webHost = app;

        // Give server a moment to start
        await Task.Delay(500);

        // Initialize scanner using shared configuration
        bool initialized = await FhirServerConfiguration.InitializeScannerAsync(app);

        return initialized;
    }

    /// <summary>
    /// Stop the FHIR API server
    /// </summary>
    public void Stop()
    {
        // Cancel the server task
        _cancellationTokenSource?.Cancel();

        // The server should stop almost immediately due to the cancellation token
        // No need to wait with StopAsync since the cancellation will handle it
        _serverTask?.Wait(TimeSpan.FromMilliseconds(500));
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        Stop();
        _cancellationTokenSource?.Dispose();
        _webHost?.Dispose();
    }
}
