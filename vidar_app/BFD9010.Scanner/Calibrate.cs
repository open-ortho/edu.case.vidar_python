/*
 * Calibrate.cs
 * Scanner calibration command.
 *
 * Responsibilities:
 *  - Execute digitizer calibration via VSCSI driver
 *  - Report calibration status to console
 *
 * Target framework: .NET 8
 */

namespace BFD9010.Scanner
{
    public class Calibrate
    {
        public static int calibrate()
        {
            DigitizeEngine digitizeEngine = new DigitizeEngine();

            try
            {
                Console.WriteLine("Calibrating...");

                int status = VscsiMethods.Calibrate();

                if (status == 0)
                {
                    Console.WriteLine("Calibration Finished");
                    return 0;
                }
                else
                {
                    Console.WriteLine($"Calibration failed with code: {status}");
                    return status;
                }

            }
            catch
            {
                throw;
            }
        }
    }
}
