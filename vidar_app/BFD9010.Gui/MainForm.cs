using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BFD9010.FhirApi;
using BFD9010.FhirApi.Services;

namespace BFD9010.Gui
{
    public partial class MainForm : Form
    {
        private ScannerService? scannerService;
        private IHost? webHost;
        private Label statusLabel = null!;
        private Label messageLabel = null!;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            _ = StartWebServerAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "BFD9010 Scanner Server";
            this.Size = new Size(400, 200);
            this.MinimizeBox = true;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
        }

        private void InitializeUI()
        {
            // Status Label
            statusLabel = new Label
            {
                Text = "Initializing...",
                Location = new Point(10, 10),
                Size = new Size(370, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.Orange,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(statusLabel);

            // Message Label
            messageLabel = new Label
            {
                Text = "Starting scanner service...",
                Location = new Point(10, 50),
                Size = new Size(370, 100),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(messageLabel);
        }

        private async Task StartWebServerAsync()
        {
            try
            {
                // Create web application using shared configuration
                var builder = WebApplication.CreateBuilder();
                
                // Add logging
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole();
                builder.Logging.SetMinimumLevel(LogLevel.Information);

                // Configure services using shared configuration
                FhirServerConfiguration.ConfigureServices(builder);

                // Build the app
                var app = builder.Build();

                // Get scanner service from DI
                scannerService = app.Services.GetRequiredService<ScannerService>();

                // Configure FHIR endpoints using shared configuration
                FhirServerConfiguration.ConfigureEndpoints(app);

                // Start the web server in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await app.RunAsync();
                    }
                    catch (Exception ex)
                    {
                        this.Invoke((Action)(() =>
                        {
                            UpdateStatus("Error", Color.Red);
                            messageLabel.Text = $"Failed to start web server:\n{ex.Message}";
                        }));
                    }
                });

                // Store the host
                webHost = app as IHost;

                // Initialize scanner using shared configuration
                await Task.Delay(500); // Give server a moment to start
                bool initialized = await FhirServerConfiguration.InitializeScannerAsync(app);

                if (initialized)
                {
                    UpdateStatus("Ready", Color.Green);
                    messageLabel.Text = $"Scanner initialized successfully!\n\n" +
                                       $"Go to https://wingate.case.edu/bfd9000/ to scan\n\n" +
                                       $"API available at http://localhost:5000";
                }
                else
                {
                    UpdateStatus("Error", Color.Red);
                    messageLabel.Text = $"Scanner initialization failed\n\n" +
                                       $"Please check the scanner connection.";
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error", Color.Red);
                messageLabel.Text = $"Failed to start:\n{ex.Message}";
            }
        }

        private void UpdateStatus(string status, Color color)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke((Action)(() => UpdateStatus(status, color)));
                return;
            }

            statusLabel.Text = status;
            statusLabel.ForeColor = color;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            // Stop web server
            if (webHost != null)
            {
                webHost.StopAsync().Wait(TimeSpan.FromSeconds(5));
                webHost.Dispose();
            }
        }
    }
}
