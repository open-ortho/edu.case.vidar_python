using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vidar_app
{
    /// <summary>
    /// Handles loading and saving scan configuration parameters from/to a config file.
    /// Uses a simple INI-like format for easy editing and debugging.
    /// </summary>
    internal class ScanConfig
    {
        private const string CONFIG_FILENAME = "scan_config.ini";

        // Scan parameters with their default values (from original code before PR)
        public short Offset0_BitDepth { get; set; } = 8;
        public short Offset2_DPI { get; set; } = 75;
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
        public short Offset56_DPI_Secondary { get; set; } = 75;
        public int Offset60_Unknown { get; set; } = 0;
        public int Offset64_Unknown { get; set; } = 0;
        public short Offset68_Unknown { get; set; } = 0;

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
            sb.AppendLine("# - These are the original default values from before the 300 DPI/16-bit changes");
            sb.AppendLine("# - Format: OffsetXX_Name = value  # description");
            sb.AppendLine("# - Lines starting with # or ; are comments");
            sb.AppendLine("# - Unknown parameters have been reverse-engineered but their exact purpose is unclear");
            sb.AppendLine("");
            
            sb.AppendLine("[ScanParameters]");
            sb.AppendLine("");
            sb.AppendLine("# Bit depth (8 or 16)");
            sb.AppendLine($"Offset0_BitDepth = {Offset0_BitDepth}");
            sb.AppendLine("");
            sb.AppendLine("# DPI resolution (common values: 75, 150, 300, 600)");
            sb.AppendLine($"Offset2_DPI = {Offset2_DPI}");
            sb.AppendLine("");
            sb.AppendLine("# Width in pixels (typically DPI * max_width_inches)");
            sb.AppendLine($"Offset4_Width = {Offset4_Width}");
            sb.AppendLine("");
            sb.AppendLine("# Height in pixels");
            sb.AppendLine($"Offset8_Height = {Offset8_Height}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown byte value");
            sb.AppendLine($"Offset12_Unknown = {Offset12_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown int value");
            sb.AppendLine($"Offset16_Unknown = {Offset16_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown short value");
            sb.AppendLine($"Offset20_Unknown = {Offset20_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Bytes per pixel (1 for 8-bit, 2 for 16-bit)");
            sb.AppendLine($"Offset24_BytesPerPixel = {Offset24_BytesPerPixel}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown short value");
            sb.AppendLine($"Offset28_Unknown = {Offset28_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown short value");
            sb.AppendLine($"Offset30_Unknown = {Offset30_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown int value");
            sb.AppendLine($"Offset32_Unknown = {Offset32_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown int value");
            sb.AppendLine($"Offset36_Unknown = {Offset36_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Output width");
            sb.AppendLine($"Offset40_OutputWidth = {Offset40_OutputWidth}");
            sb.AppendLine("");
            sb.AppendLine("# Output height");
            sb.AppendLine($"Offset44_OutputHeight = {Offset44_OutputHeight}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown int value");
            sb.AppendLine($"Offset48_Unknown = {Offset48_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# DPI resolution (secondary, should match Offset2_DPI)");
            sb.AppendLine($"Offset56_DPI_Secondary = {Offset56_DPI_Secondary}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown int value");
            sb.AppendLine($"Offset60_Unknown = {Offset60_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown int value");
            sb.AppendLine($"Offset64_Unknown = {Offset64_Unknown}");
            sb.AppendLine("");
            sb.AppendLine("# Unknown short value");
            sb.AppendLine($"Offset68_Unknown = {Offset68_Unknown}");
            
            File.WriteAllText(path, sb.ToString());
            Console.WriteLine($"Configuration saved to: {path}");
        }

        private void SetValue(string key, string value)
        {
            try
            {
                switch (key)
                {
                    case "Offset0_BitDepth":
                        Offset0_BitDepth = short.Parse(value);
                        break;
                    case "Offset2_DPI":
                        Offset2_DPI = short.Parse(value);
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
                    case "Offset56_DPI_Secondary":
                        Offset56_DPI_Secondary = short.Parse(value);
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
            // Place config file in the same directory as the executable
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(exeDir, CONFIG_FILENAME);
        }
    }
}
