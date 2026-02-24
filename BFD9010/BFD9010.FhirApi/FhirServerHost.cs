using BFD9010.Scanner;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Hosts the FHIR API web server and exposes the shared ScannerService instance.
namespace BFD9010.FhirApi;

/// <summary>
/// Manages the lifecycle of the FHIR API server for both CLI and GUI applications
/// </summary>
public class FhirServerHost : IDisposable, IAsyncDisposable
{
    private IHost? _webHost;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _serverTask;
    private readonly string? _configPath;
    private bool _disposed;
    private Services.ScannerService? _scannerService;

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

        _scannerService = app.Services.GetRequiredService<Services.ScannerService>();

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

    public Services.ScannerService? ScannerService => _scannerService;

    /// <summary>
    /// Stop the FHIR API server
    /// </summary>
    public void Stop()
    {
        if (_disposed)
            return;

        // Cancel the server task
        _cancellationTokenSource?.Cancel();

        // Don't use Wait() as it can cause deadlocks
        // The cancellation token will handle stopping the server
    }

    /// <summary>
    /// Stop the FHIR API server asynchronously
    /// </summary>
    public async Task StopAsync()
    {
        if (_disposed)
            return;

        // Cancel the server task
        _cancellationTokenSource?.Cancel();

        // Wait for the server task to complete
        if (_serverTask != null)
        {
            try
            {
                await _serverTask.WaitAsync(TimeSpan.FromMilliseconds(500));
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping the server
            }
            catch (TimeoutException)
            {
                // Server didn't stop in time, but we've cancelled it
            }
        }
    }

    /// <summary>
    /// Dispose of resources asynchronously (preferred method)
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await StopAsync();
        
        _cancellationTokenSource?.Dispose();
        _webHost?.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of resources synchronously
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        
        _cancellationTokenSource?.Dispose();
        _webHost?.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
