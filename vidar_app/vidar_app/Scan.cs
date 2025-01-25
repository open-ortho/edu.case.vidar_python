using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static vidar_app.VscsiTypes;
using static vidar_app.Scanner;
using static vidar_app.VscsiMethods;
using System.ComponentModel;

namespace vidar_app
{
    internal class Scan
    {
        public unsafe static int scan(_DIGITIZERINFO digitizerInfo, ScannerData scanner_data)
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();

            try
            {
                digitizeEngine = digitizeEngine2;

                _SCANPARAMETERS scan_parameters = digitizeEngine.InitScanParams();

                // Guess, this multiplies DPI and max width to get the width of the image.
                scan_parameters.Field4 = (short)(scan_parameters.Field2 * scanner_data.maxWidthInInches);

                // Unsure what the Max_inches value is, but I calculated it to be 51 with the default values.
                //digitizeEngine.scan_parameters.Field8 = digitizeEngine.scan_parameters.Field56 * ?Max_Inches?

                // To be honest not sure what this one does, but once again matches with the hardcoded defaults.
                scan_parameters.Field24 = (short)Math.Ceiling((double)scan_parameters.Field0);

                // Not sure what this one does either, i think it turns into scanByteCount though.
                scan_parameters.Field52 = 0;

                // 8400*30600*2=514,080,000, persumably width*heigh*channels or something.
                /**int imageBufferSize = scan_parameters.Field4 *
                                      scan_parameters.Field8 *
                                      scan_parameters.Field24;**/
                int imageBufferSize = 14 * 51 * 600;



                byte[] imageBuffer = new byte[imageBufferSize];
                Array.Clear(imageBuffer, 0, imageBufferSize);

                uint totalBytesRecieved = 0;

                int status = -1;

                //int status = Scan(ref digitizerInfo, ref scan_parameters, ref imageBuffer, ref totalBytesRecieved);

                Thread thread = new Thread(() => DigitizeEngine.StartScan(ref status, ref digitizerInfo, ref scan_parameters, ref imageBuffer, ref totalBytesRecieved));
                thread.Start();
                thread.Join();

                Console.WriteLine(status);

                ushort num3 = 0;
                _VIDARERRORINFO errInfo = new _VIDARERRORINFO();
                short s = getVidarError(status, ref errInfo, ref num3);

                Console.WriteLine(Encoding.ASCII.GetString(errInfo.Data));
                return 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}
