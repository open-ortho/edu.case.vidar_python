using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static vidar_app.VscsiTypes;


namespace vidar_app
{
    internal class Eject
    {
        public static int eject(VscsiTypes._DIGITIZERINFO dIGITIZERINFO)
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();

            try
            {
                digitizeEngine = digitizeEngine2;

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
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
