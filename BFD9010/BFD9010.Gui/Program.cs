using System;
using System.Windows.Forms;

// Application entry point and startup error reporting.
namespace BFD9010.Gui
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Parse command-line args for optional --config <path>
            string? configPath = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--config", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    configPath = args[i + 1];
                    i++; // skip next
                }
            }

            try
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.Run(new MainForm(configPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show(BuildStartupErrorMessage(ex.Message), "BFD9010 Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string BuildStartupErrorMessage(string details)
        {
            return "Startup failed:\n" + details +
                   "\n\nPlease ensure:\n" +
                   "1. Vidar drivers have been installed\n" +
                   "2. No other Vidar software is running (another instance of this app or Vidar scanning software)";
        }
    }
}
