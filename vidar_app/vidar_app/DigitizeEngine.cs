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
        public _SCANPARAMETERS scan_parameters;


        public int GetDigInfo(ref _DIGITIZERINFO digitizerInfo)
        {
            // Call the external method to populate the structure
            int status = VscsiMethods.getDigitizerInfo(ref digitizerInfo);

            // Return the status code
            return status;
        }


        public void InitScanParams()
        {
            scan_parameters.Field0 = 16;
            scan_parameters.Field2 = 600;
            scan_parameters.Field56 = 600;
            scan_parameters.Field4 = 8400;
            scan_parameters.Field8 = 30600;
            scan_parameters.Field12 = 2;
            scan_parameters.Field24 = 2;
            scan_parameters.Field28 = 1;
            scan_parameters.Field30 = 0;
            scan_parameters.Field32 = 1;
            scan_parameters.Field36 = 1;
            scan_parameters.Field16 = 1;
            scan_parameters.Field20 = 1;
            scan_parameters.Field44 = 30600;
            scan_parameters.Field40 = 8400;
            scan_parameters.Field48 = 0;
            scan_parameters.Field60 = 0;
            scan_parameters.Field64 = 0;
            scan_parameters.Field68 = 0;
        }
    }

}
