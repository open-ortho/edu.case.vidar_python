using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static vidar_app.VscsiTypes;


namespace vidar_app
{
    internal class DigitizeEngine
    {
        public static int ErrorCode = 0;
        private _SCANPARAMETERS sp;


        public int GetDigInfo(ref _DIGITIZERINFO digitizerInfo)
        {
            // Call the external method to populate the structure
            int status = VscsiMethods.getDigitizerInfo(ref digitizerInfo);

            // Return the status code
            return status;
        }


        public void InitScanParams()
        {
            sp.Field0 = 16;
            sp.Field2 = 600;
            sp.Field56 = 600;
            sp.Field4 = 8400;
            sp.Field8 = 30600;
            sp.Field12 = 2;
            sp.Field24 = 2;
            sp.Field28 = 1;
            sp.Field30 = 0;
            sp.Field32 = 1;
            sp.Field36 = 1;
            sp.Field16 = 1;
            sp.Field20 = 1;
            sp.Field44 = 30600;
            sp.Field40 = 8400;
            sp.Field48 = 0;
            sp.Field60 = 0;
            sp.Field64 = 0;
            sp.Field68 = 0;
        }
        public void Normalize()
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();
                // Assign digitizeEngine to the new instance
                digitizeEngine = digitizeEngine2;

                // Call the external method Calibrate and set the ErrorCode
                DigitizeEngine.ErrorCode = VscsiMethods.Calibrate();
                Console.WriteLine(ErrorCode);

                //if (DigitizeEngine.ErrorCode != 0)
                {
                    // If calibration failed, call CheckError
                    //CheckError();
                }
        }

            /*
            public void Eject()
            {
                // Declare and initialize GETDIGINFO structure
                var getDigitInfo = new _GETDIGINFO();

                // Call to getDigitizerInfo to populate the structure
                int result = VscsiMethods.getDigitizerInfo(ref getDigitInfo);

                // Check if the call was successful
                if (result != 0)
                {
                    Console.WriteLine("WARNING: could not read the digitizer's capabilities.", "VIDARscan NDTPRO");
                    return;
                }

                // Copy the digitizer information from GETDIGINFO to DIGITIZERINFO
                var digitizerInfo = new _DIGITIZERINFO();
                Buffer.BlockCopy(getDigitInfo.Data, 0, digitizerInfo.Data, 0, sizeof(_DIGITIZERINFO));

                // Eject the film and check for errors
                ErrorCode = VscsiMethods.EjectFilm(ref digitizerInfo, 0);
                if (ErrorCode != 0)
                {
                    //CheckError();
                }
            }*/

        }
}
