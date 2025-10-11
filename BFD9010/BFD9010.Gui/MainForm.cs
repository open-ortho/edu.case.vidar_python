using BFD9010.FhirApi;
using BFD9010.Scanner;
using System.Diagnostics;
using System.Reflection;

namespace BFD9010.Gui
{
    public partial class MainForm : Form
    {
        private const string LogoResourceName = "BFD9010.Gui.Resources.BFD9000_logo_white.png";
        
        private readonly string? _configPath;
        private FhirServerHost? _serverHost;
        private Label statusLabel = null!;
        private Label messageLabel = null!;
        private LinkLabel urlLinkLabel = null!;
        private Label apiLabel = null!;

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
            this.Size = new Size(400, 350);
            this.MinimizeBox = true;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.BackColor = Color.White;
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
                TextAlign = ContentAlignment.MiddleCenter
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
                TextAlign = ContentAlignment.MiddleCenter
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
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(statusLabel);

            // Message Label (for status/error messages)
            messageLabel = new Label
            {
                Text = "Starting scanner service...",
                Location = new Point(10, 195),
                Size = new Size(370, 60),
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(messageLabel);

            // URL Link Label
            urlLinkLabel = new LinkLabel
            {
                Text = "Loading...",
                Location = new Point(10, 265),
                Size = new Size(370, 30),
                Font = new Font("Segoe UI", 11F, FontStyle.Underline),
                TextAlign = ContentAlignment.MiddleCenter,
                LinkColor = Color.Blue,
                VisitedLinkColor = Color.Purple,
                ActiveLinkColor = Color.Red
            };
            urlLinkLabel.LinkClicked += UrlLinkLabel_LinkClicked;
            this.Controls.Add(urlLinkLabel);

            // API Info Label
            apiLabel = new Label
            {
                Text = "",
                Location = new Point(10, 305),
                Size = new Size(370, 40),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.TopCenter
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
                var config = ScanConfig.Load(_configPath);

                // Create and start the server host
                _serverHost = new FhirServerHost(_configPath);
                bool initialized = await _serverHost.StartAsync();

                if (initialized)
                {
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
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error", Color.Red);
                messageLabel.Text = $"Failed to start:\n{ex.Message}";
                urlLinkLabel.Text = "Service failed to start";
                urlLinkLabel.Enabled = false;
            }
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
            
            // Stop and dispose the server host (fast shutdown)
            _serverHost?.Dispose();
        }
    }
}
