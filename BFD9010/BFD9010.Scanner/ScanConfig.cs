using System.Text;

namespace BFD9010.Scanner
{
    /// <summary>
    /// Handles loading and saving scan configuration parameters from/to a config file.
    /// Uses a simple INI-like format for easy editing and debugging.
    /// </summary>
    public class ScanConfig
    {
        private const string CONFIG_FILENAME = "~\\Desktop\\VidarScans\\scan_config.ini";

        // Scan parameters with their default values (from original code before PR)
        public short Offset0_BitDepth { get; set; } = 16;
        // Keep internal Offset2 name but the INI exposes a single 'DPI' key
        public short Offset2_DPI_X { get; set; } = 300;
        public short Offset4_Width { get; set; } = 1050;
        public int Offset8_Height { get; set; } = 3825;
        public sbyte Offset12_Unknown { get; set; } = 2;
        public int Offset16_Unknown { get; set; } = 1;
        public short Offset20_Unknown { get; set; } = 1;
        public int Offset24_BytesPerPixel { get; set; } = 1;
        public short Offset28_Unknown { get; set; } = 1;
        public short Offset30_Unknown { get; set; } = 0;
        public int Offset32_Unknown { get; set; } = 1;
        public int Offset36_Unknown { get; set; } = 1;
        public short Offset40_OutputWidth { get; set; } = 8400;
        public int Offset44_OutputHeight { get; set; } = 30600;
        public int Offset48_Unknown { get; set; } = 0;
        // Y-Axis DPI is kept for internal completeness but is derived from Offset2_DPI
        public short Offset56_DPI_Y { get; set; } = 300;
        public int Offset60_Unknown { get; set; } = 0;
        public int Offset64_Unknown { get; set; } = 0;
        public short Offset68_Unknown { get; set; } = 0;

        // New output options
        // OutputPath: directory where images will be written (default current directory)
        public string OutputPath { get; set; } = "~\\Desktop\\VidarScans";
        // OutputPrefix: filename prefix template. Use ${DPI} and ${BIT} tokens.
        public string OutputPrefix { get; set; } = "${DPI}DPI_${BIT}BIT";

        // Web API settings
        // WebAppUrl: URL of the web application that users should navigate to
        public string WebAppUrl { get; set; } = "https://wingate.case.edu/bfd9000/";
        // CorsOrigin: CORS origin(s) to allow (comma-separated for multiple origins)
        public string CorsOrigin { get; set; } = "https://wingate.case.edu";

        /// <summary>
        /// Loads configuration from file. If file doesn't exist, creates it with defaults.
        /// If configPath is null, uses default location next to the executable.
        /// </summary>
        public static ScanConfig Load(string? configPath = null)
        {
            string path = configPath ?? GetConfigPath();

            if (!File.Exists(path))
            {
                Console.WriteLine($"Config file not found at: {path}");
                Console.WriteLine("Creating default config file with original parameters...");
                ScanConfig defaultConfig = new ScanConfig();
                defaultConfig.Save(path);
                return defaultConfig;
            }

            Console.WriteLine($"Loading scan configuration from: {path}");
            ScanConfig config = new ScanConfig();

            try
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    // Skip comments and empty lines
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";"))
                        continue;

                    // Parse key=value pairs
                    int equalsIndex = trimmed.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = trimmed.Substring(0, equalsIndex).Trim();
                        string value = trimmed.Substring(equalsIndex + 1).Trim();

                        // Remove inline comments
                        int commentIndex = value.IndexOf('#');
                        if (commentIndex >= 0)
                            value = value.Substring(0, commentIndex).Trim();

                        config.SetValue(key, value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config file: {ex.Message}");
                Console.WriteLine("Using default values.");
            }

            // Ensure secondary DPI equals primary DPI — keep only one DPI in the config semantics
            config.Offset56_DPI_Y = config.Offset2_DPI_X;

            // Print the resolved output configuration so user knows where images will be written
            try
            {
                string resolvedPath = ExpandPath(config.OutputPath ?? ".");
                Console.WriteLine($"Output configuration: Format=PNG, Path={resolvedPath}, Prefix={config.OutputPrefix}");
            }
            catch
            {
                Console.WriteLine($"Output configuration: Format=PNG, Path={config.OutputPath}, Prefix={config.OutputPrefix}");
            }

            return config;
        }

        /// <summary>
        /// Saves current configuration to file with comments explaining each parameter.
        /// If configPath is null, saves next to the executable.
        /// </summary>
        public void Save(string? configPath = null)
        {
            string path = configPath ?? GetConfigPath();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Vidar Scanner Configuration");
            sb.AppendLine("# This file controls scan parameters sent to the scanner hardware.");
            sb.AppendLine("# Edit values to test different settings, then restart the app to reload.");
            sb.AppendLine("#");
            sb.AppendLine("# NOTES:");
            sb.AppendLine("# - These are default values for 300 DPI/16-bit");
            sb.AppendLine("# - Format: key = value  # description");
            sb.AppendLine("# - Lines starting with # or ; are comments");
            //sb.AppendLine("# - Unknown parameters have been reverse-engineered but their exact purpose is unclear");
            sb.AppendLine("");

            sb.AppendLine("[ScanParameters]");
            sb.AppendLine("");
            sb.AppendLine("# Bit depth (8 or 16)");
            sb.AppendLine($"BitDepth = {Offset0_BitDepth}");
            sb.AppendLine("");
            sb.AppendLine("# DPI resolution (tested DPIs: 75, 150, 300)");
            sb.AppendLine($"DPI = {Offset2_DPI_X}");
            sb.AppendLine("");
            //sb.AppendLine("# Width in pixels (typically DPI * max_width_inches)");
            //sb.AppendLine($"Offset4_Width = {Offset4_Width}");
            //sb.AppendLine("");
            //sb.AppendLine("# Height in pixels");
            //sb.AppendLine($"Offset8_Height = {Offset8_Height}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown byte value");
            //sb.AppendLine($"Offset12_Unknown = {Offset12_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown int value");
            //sb.AppendLine($"Offset16_Unknown = {Offset16_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown short value");
            //sb.AppendLine($"Offset20_Unknown = {Offset20_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Bytes per pixel (1 for 8-bit, 2 for 16-bit)");
            //sb.AppendLine($"Offset24_BytesPerPixel = {Offset24_BytesPerPixel}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown short value");
            //sb.AppendLine($"Offset28_Unknown = {Offset28_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown short value");
            //sb.AppendLine($"Offset30_Unknown = {Offset30_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown int value");
            //sb.AppendLine($"Offset32_Unknown = {Offset32_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown int value");
            //sb.AppendLine($"Offset36_Unknown = {Offset36_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Output width");
            //sb.AppendLine($"Offset40_OutputWidth = {Offset40_OutputWidth}");
            //sb.AppendLine("");
            //sb.AppendLine("# Output height");
            //sb.AppendLine($"Offset44_OutputHeight = {Offset44_OutputHeight}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown int value");
            //sb.AppendLine($"Offset48_Unknown = {Offset48_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown int value");
            //sb.AppendLine($"Offset60_Unknown = {Offset60_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown int value");
            //sb.AppendLine($"Offset64_Unknown = {Offset64_Unknown}");
            //sb.AppendLine("");
            //sb.AppendLine("# Unknown short value");
            //sb.AppendLine($"Offset68_Unknown = {Offset68_Unknown}");
            //sb.AppendLine("");
            sb.AppendLine("# Output options");
            sb.AppendLine("# OutputPath: directory where image files will be written (PNG format)");
            sb.AppendLine($"OutputPath = {OutputPath}");
            sb.AppendLine("");
            sb.AppendLine("# OutputPrefix: filename prefix template. Tokens: ${DPI}, ${BIT}");
            sb.AppendLine($"OutputPrefix = {OutputPrefix}");
            sb.AppendLine("");
            sb.AppendLine("# Web API settings");
            sb.AppendLine("# WebAppUrl: URL of the web application users should navigate to for scanning");
            sb.AppendLine($"WebAppUrl = {WebAppUrl}");
            sb.AppendLine("");
            sb.AppendLine("# CorsOrigin: CORS origin(s) to allow API access from (comma-separated for multiple)");
            sb.AppendLine($"CorsOrigin = {CorsOrigin}");

            File.WriteAllText(path, sb.ToString());
            Console.WriteLine($"Configuration saved to: {path}");
        }

        private void SetValue(string key, string value)
        {
            try
            {
                switch (key)
                {
                    case "BitDepth":
                        Offset0_BitDepth = short.Parse(value);
                        break;
                    case "DPI":
                        // new, single DPI key for INI
                        Offset2_DPI_X = short.Parse(value);
                        Offset56_DPI_Y = short.Parse(value);
                        break;
                    case "Offset4_Width":
                        Offset4_Width = short.Parse(value);
                        break;
                    case "Offset8_Height":
                        Offset8_Height = int.Parse(value);
                        break;
                    case "Offset12_Unknown":
                        Offset12_Unknown = sbyte.Parse(value);
                        break;
                    case "Offset16_Unknown":
                        Offset16_Unknown = int.Parse(value);
                        break;
                    case "Offset20_Unknown":
                        Offset20_Unknown = short.Parse(value);
                        break;
                    case "Offset24_BytesPerPixel":
                        Offset24_BytesPerPixel = int.Parse(value);
                        break;
                    case "Offset28_Unknown":
                        Offset28_Unknown = short.Parse(value);
                        break;
                    case "Offset30_Unknown":
                        Offset30_Unknown = short.Parse(value);
                        break;
                    case "Offset32_Unknown":
                        Offset32_Unknown = int.Parse(value);
                        break;
                    case "Offset36_Unknown":
                        Offset36_Unknown = int.Parse(value);
                        break;
                    case "Offset40_OutputWidth":
                        Offset40_OutputWidth = short.Parse(value);
                        break;
                    case "Offset44_OutputHeight":
                        Offset44_OutputHeight = int.Parse(value);
                        break;
                    case "Offset48_Unknown":
                        Offset48_Unknown = int.Parse(value);
                        break;
                    case "Offset60_Unknown":
                        Offset60_Unknown = int.Parse(value);
                        break;
                    case "Offset64_Unknown":
                        Offset64_Unknown = int.Parse(value);
                        break;
                    case "Offset68_Unknown":
                        Offset68_Unknown = short.Parse(value);
                        break;
                    case "OutputPath":
                        OutputPath = value;
                        break;
                    case "OutputPrefix":
                        OutputPrefix = value;
                        break;
                    case "WebAppUrl":
                        WebAppUrl = value;
                        break;
                    case "CorsOrigin":
                        CorsOrigin = value;
                        break;
                    default:
                        Console.WriteLine($"Warning: Unknown config key: {key}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing value for {key}: {ex.Message}");
            }
        }

        private static string GetConfigPath()
        {
            string outDir = Environment.ExpandEnvironmentVariables(CONFIG_FILENAME);
            if (outDir.StartsWith('~'))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string rest = outDir.Length == 1 ? string.Empty : outDir.Substring(1).TrimStart('\\', '/');
                outDir = Path.Combine(home, rest);
            }
            outDir = Path.GetFullPath(outDir);
            Directory.CreateDirectory(Path.GetDirectoryName(outDir));
            return outDir;
        }

        private static string ExpandPath(string path)
        {
            // Expand user home directory (~) and environment variables
            if (string.IsNullOrWhiteSpace(path))
                return path ?? string.Empty;

            try
            {
                string p = path.Trim();

                // Expand environment variables (e.g., %USERPROFILE%) first
                p = Environment.ExpandEnvironmentVariables(p);

                // Expand ~ to user profile folder
                if (p.StartsWith("~"))
                {
                    string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string rest = p.Length == 1 ? string.Empty : p.Substring(1).TrimStart('\\', '/');
                    p = Path.Combine(homePath, rest);
                }

                // Return full absolute path
                return Path.GetFullPath(p);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error expanding path: {ex.Message}");
                return path;
            }
        }
    }
}
