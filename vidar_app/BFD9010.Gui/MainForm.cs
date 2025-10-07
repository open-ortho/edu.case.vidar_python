using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BFD9010.FhirApi.Services;

namespace BFD9010.Gui
{
    public partial class MainForm : Form
    {
        private Label statusLabel;
        private Label messageLabel;
        private IHost? webHost;
        private ScannerService? scannerService;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            StartWebServer();
        }

        private void InitializeComponent()
        {
            this.Text = "BFD9010 Scanner Server";
            this.Size = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Status label
            statusLabel = new Label
            {
                Text = "Initializing...",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.Orange,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 15, 0, 0)
            };

            // Message label
            messageLabel = new Label
            {
                Text = "Please wait while the scanner initializes...",
                Font = new Font("Segoe UI", 10F),
                AutoSize = false,
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 20)
            };

            this.Controls.Add(messageLabel);
            this.Controls.Add(statusLabel);
        }

        private void InitializeUI()
        {
            // Additional UI initialization if needed
        }

        private async void StartWebServer()
        {
            try
            {
                UpdateStatus("Initializing...", Color.Orange);
                
                // Create and start the web host
                var builder = WebApplication.CreateBuilder();
                
                // Configure services
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddSingleton<ScannerService>();
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("WingatePolicy", policy =>
                    {
                        policy.WithOrigins("https://wingate.case.edu")
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });

                // Configure Kestrel to listen on port 5000
                builder.WebHost.UseUrls("http://localhost:5000");

                var app = builder.Build();
                
                // Configure the HTTP request pipeline
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors("WingatePolicy");

                // Get scanner service and initialize
                scannerService = app.Services.GetRequiredService<ScannerService>();
                
                // Register FHIR endpoints (simplified - full implementation would be in separate file)
                ConfigureFhirEndpoints(app);

                // Start web server in background
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

                // Initialize scanner
                await Task.Delay(500); // Give server a moment to start
                int initStatus = await scannerService.InitializeAsync();

                if (initStatus == 0)
                {
                    UpdateStatus("Ready", Color.Green);
                    messageLabel.Text = $"Scanner initialized successfully!\n\n" +
                                       $"Go to https://wingate.case.edu/bfd9000/ to scan\n\n" +
                                       $"API available at http://localhost:5000";
                }
                else
                {
                    UpdateStatus("Error", Color.Red);
                    messageLabel.Text = $"Scanner initialization failed (code: {initStatus})\n\n" +
                                       $"Please check the scanner connection.";
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error", Color.Red);
                messageLabel.Text = $"Failed to start:\n{ex.Message}";
            }
        }

        private void ConfigureFhirEndpoints(WebApplication app)
        {
            // Import endpoints from FHIR API project
            // For now, registering simplified versions
            app.MapGet("/Device/{id}", (string id) => 
            {
                if (scannerService == null || !scannerService.IsInitialized)
                {
                    return Results.Problem("Scanner not initialized");
                }
                return Results.Ok(new { message = "Device endpoint", id });
            });

            app.MapPost("/Device/{id}/$scan", async (string id) =>
            {
                if (scannerService == null || !scannerService.IsInitialized)
                {
                    return Results.Problem("Scanner not initialized");
                }

                this.Invoke((Action)(() => UpdateStatus("Scanning...", Color.Blue)));

                var (status, imageBytes) = await scannerService.ScanAsync();

                this.Invoke((Action)(() => UpdateStatus("Ready", Color.Green)));

                if (status == 0 && imageBytes != null)
                {
                    return Results.Ok(new
                    {
                        resourceType = "Bundle",
                        type = "collection",
                        entry = new[]
                        {
                            new { resource = new { resourceType = "Binary", contentType = "image/png", data = Convert.ToBase64String(imageBytes) } }
                        }
                    });
                }
                else
                {
                    return Results.Problem($"Scan failed with code {status}");
                }
            });
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
