/*
 * Eject.cs
 * Film eject command for scanner feeder.
 *
 * Responsibilities:
 *  - Eject film from the digitizer feeder mechanism
 *  - Report eject status to console
 *
 * Target framework: .NET 8
 */

namespace vidar_app
{
    internal class Eject
    {
        public static int eject(VscsiTypes._DIGITIZERINFO dIGITIZERINFO)
        {
            DigitizeEngine digitizeEngine = new DigitizeEngine();

            try
            {
                Console.WriteLine("Ejecting film...");

                int status = VscsiMethods.EjectFilm(ref dIGITIZERINFO, 0);

                if (status == 0)
                {
                    Console.WriteLine("Film Ejected");
                    return 0;
                }
                else
                {
                    Console.WriteLine($"Ejection failed with code: {status}");
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
