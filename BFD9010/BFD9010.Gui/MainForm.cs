using BFD9010.FhirApi;
using BFD9010.FhirApi.Services;
using BFD9010.Scanner;
using System.Diagnostics;
using System.Reflection;

// GUI status window for scanner and FHIR API server.
namespace BFD9010.Gui
{
    public partial class MainForm : Form
    {
        private const string LogoResourceName = "BFD9010.Gui.Resources.BFD9000_logo_white.png";
        private const string IconResourceName = "BFD9010.Gui.Resources.BFD9000_logo_white.ico";
        
        private readonly string? _configPath;
        private FhirServerHost? _serverHost;
        private Label statusLabel = null!;
        private Label messageLabel = null!;
        private Label configLabel = null!;
        private LinkLabel urlLinkLabel = null!;
        private Label apiLabel = null!;
        private ScannerService? _scannerService;
        private bool _statusSubscribed;

        public MainForm(string? configPath = null)
        {
            _configPath = configPath;
            InitializeComponent();
            InitializeUI();
            _ = StartWebServerAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "BFD9010 Scanner Server";
            this.Size = new Size(400, 380);
            this.MinimizeBox = true;
            this.MaximizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(360, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.BackColor = Color.White;
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            
            // Set the form icon
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(IconResourceName))
                {
                    if (stream != null)
                    {
                        this.Icon = new Icon(stream);
                    }
                    else
                    {
                        string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BFD9000_logo_white.ico");
                        if (File.Exists(iconPath))
                        {
                            this.Icon = new Icon(iconPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load icon: {ex.Message}");
            }
        }

        private void InitializeUI()
        {
            // Logo PictureBox
            var logoPictureBox = new PictureBox
            {
                Location = new Point(100, 10),
                Size = new Size(200, 80),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            
            // Load the embedded logo image
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(LogoResourceName))
                {
                    if (stream != null)
                    {
                        logoPictureBox.Image = Image.FromStream(stream);
                        this.Controls.Add(logoPictureBox);
                    }
                }
            }
            catch (Exception ex)
            {
                // If logo fails to load, just skip it, but log the error for debugging
                Debug.WriteLine($"Failed to load logo: {ex.Message}\n{ex.StackTrace}");
            }

            // Project Name Label
            var projectNameLabel = new Label
            {
                Text = "BFD9010: An HL7 FHIR API for scanners",
                Location = new Point(10, 100),
                Size = new Size(370, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(projectNameLabel);

            // Version Label
            var versionLabel = new Label
            {
                Text = $"Version {GetVersionString()}",
                Location = new Point(10, 125),
                Size = new Size(370, 20),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(versionLabel);

            // Status Label
            statusLabel = new Label
            {
                Text = "Initializing...",
                Location = new Point(10, 155),
                Size = new Size(370, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.Orange,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(statusLabel);

            // Message Label (for status/error messages)
            messageLabel = new Label
            {
                Text = "Starting scanner service...",
                Location = new Point(10, 195),
                Size = new Size(370, 45),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.TopCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(messageLabel);

            // Config Label (DPI/Bit Depth)
            configLabel = new Label
            {
                Text = "Scan settings: --",
                Location = new Point(10, 240),
                Size = new Size(370, 35),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(configLabel);

            // URL Link Label
            urlLinkLabel = new LinkLabel
            {
                Text = "Loading...",
                Location = new Point(10, 280),
                Size = new Size(370, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Underline),
                TextAlign = ContentAlignment.MiddleCenter,
                LinkColor = Color.Blue,
                VisitedLinkColor = Color.Purple,
                ActiveLinkColor = Color.Red,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            urlLinkLabel.LinkClicked += UrlLinkLabel_LinkClicked;
            this.Controls.Add(urlLinkLabel);

            // API Info Label
            apiLabel = new Label
            {
                Text = "",
                Location = new Point(10, 315),
                Size = new Size(370, 40),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(apiLabel);
        }

        private void UrlLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(urlLinkLabel.Tag as string))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = urlLinkLabel.Tag as string,
                        UseShellExecute = true
                    });
                    urlLinkLabel.LinkVisited = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task StartWebServerAsync()
        {
            try
            {
                // Load configuration to display the web URL
                string resolvedConfigPath = _configPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scan_config.ini");
                bool configExists = File.Exists(resolvedConfigPath);
                var config = ScanConfig.Load(_configPath);

                string settingsText = $"Scan settings: {config.Offset2_DPI_X} DPI / {config.Offset0_BitDepth}-bit";
                string configText = configExists
                    ? $"Config: {resolvedConfigPath}"
                    : $"Config: defaults (created {resolvedConfigPath})";
                configLabel.Text = $"{settingsText}\n{configText}";

                // Create and start the server host
                _serverHost = new FhirServerHost(_configPath);
                bool initialized = await _serverHost.StartAsync();
                HookStatusUpdates();

                if (initialized)
                {
                    ShowUi();
                    UpdateStatus("Ready", Color.Green);
                    messageLabel.Text = "Scanner initialized successfully!";
                    
                    // Set the clickable link from config
                    string webUrl = config.WebAppUrl ?? "https://wingate.case.edu/bfd9000/";
                    urlLinkLabel.Text = $"Click here to scan: {webUrl}";
                    urlLinkLabel.Tag = webUrl;
                    
                    // Show API info
                    apiLabel.Text = "API available at http://localhost:5000";
                }
                else
                {
                    UpdateStatus("Error", Color.Red);
                    messageLabel.Text = "Scanner initialization failed\n\nPlease check the scanner connection.";
                    urlLinkLabel.Text = "Scanner not ready";
                    urlLinkLabel.Enabled = false;
                    ShowStartupError("Scanner initialization failed.");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error", Color.Red);
                messageLabel.Text = $"Failed to start:\n{ex.Message}";
                urlLinkLabel.Text = "Service failed to start";
                urlLinkLabel.Enabled = false;
                ShowStartupError(ex.Message);
            }
        }

        private void HookStatusUpdates()
        {
            if (_statusSubscribed || _serverHost == null)
            {
                return;
            }

            _scannerService = _serverHost.ScannerService;
            if (_scannerService == null)
            {
                return;
            }

            _scannerService.StatusChanged += OnStatusChanged;
            _statusSubscribed = true;
            ApplyStatus(_scannerService.CurrentStatus, null);
        }

        private void OnStatusChanged(ScannerStatus status, string? details)
        {
            ApplyStatus(status, details);
        }

        private void ApplyStatus(ScannerStatus status, string? details)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => ApplyStatus(status, details)));
                return;
            }

            switch (status)
            {
                case ScannerStatus.Ready:
                    UpdateStatus("Ready", Color.Green);
                    if (!string.IsNullOrWhiteSpace(details))
                    {
                        messageLabel.Text = details;
                    }
                    break;
                case ScannerStatus.Scanning:
                    UpdateStatus("Scanning", Color.Orange);
                    break;
                case ScannerStatus.Calibrating:
                    UpdateStatus("Calibrating", Color.Orange);
                    break;
                case ScannerStatus.Ejecting:
                    UpdateStatus("Ejecting", Color.Orange);
                    break;
                case ScannerStatus.Error:
                    UpdateStatus("Error", Color.Red);
                    if (!string.IsNullOrWhiteSpace(details))
                    {
                        messageLabel.Text = details;
                    }
                    break;
                case ScannerStatus.Initializing:
                    UpdateStatus("Initializing", Color.Orange);
                    break;
            }
        }

        private void ShowUi()
        {
            if (InvokeRequired)
            {
                Invoke((Action)ShowUi);
                return;
            }

            this.ShowInTaskbar = true;
            this.Opacity = 1;
        }

        private void ShowStartupError(string details)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() => ShowStartupError(details)));
                return;
            }

            string message = "Startup failed:\n" + details +
                             "\n\nPlease ensure:\n" +
                             "1. Vidar drivers have been installed\n" +
                             "2. No other Vidar software is running (another instance of this app or Vidar scanning software)";
            MessageBox.Show(this, message, "BFD9010 Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }

        private static string GetVersionString()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            
            if (string.IsNullOrWhiteSpace(infoVersion))
                return "Unknown";
            
            // Remove build metadata (everything after '+')
            var plusIndex = infoVersion.IndexOf('+');
            if (plusIndex >= 0)
                infoVersion = infoVersion.Substring(0, plusIndex);
            
            return infoVersion;
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
            
            // Stop the server synchronously (cancels immediately without waiting)
            // This is safe in OnFormClosing since cancellation is instant
            _serverHost?.Stop();
            
            // Note: We call Stop() instead of Dispose() to avoid blocking the UI thread.
            // The actual disposal will happen when the form is disposed.
            if (_statusSubscribed && _scannerService != null)
            {
                _scannerService.StatusChanged -= OnStatusChanged;
                _statusSubscribed = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose the server host when the form is disposed
                _serverHost?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
