using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static vidar_app.VscsiTypes;
using static vidar_app.Scanner;
using static vidar_app.VscsiMethods;

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

                digitizeEngine.InitScanParams();

                // Guess, this multiplies DPI and max width to get the width of the image.
                digitizeEngine.scan_parameters.Field4 = (short)(digitizeEngine.scan_parameters.Field2 * scanner_data.maxWidthInInches);

                // Unsure what the Max_inches value is, but I calculated it to be 51 with the default values.
                //digitizeEngine.scan_parameters.Field8 = digitizeEngine.scan_parameters.Field56 * ?Max_Inches?

                // To be honest not sure what this one does, but once again matches with the hardcoded defaults.
                digitizeEngine.scan_parameters.Field24 = (short)Math.Ceiling((double)digitizeEngine.scan_parameters.Field0);

                // Not sure what this one does either, i think it turns into scanByteCount though.
                digitizeEngine.scan_parameters.Field52 = 0;

                // 8400*30600*2=514,080,000, persumably width*heigh*channels or something.
                int imageBufferSize = digitizeEngine.scan_parameters.Field4 *
                                      digitizeEngine.scan_parameters.Field8 *
                                      digitizeEngine.scan_parameters.Field24;
                
                
                byte* imageBuffer = stackalloc byte[14*51*600];

                _SCANFILM sCANFILM = new _SCANFILM();
                int num2 = 0;
                uint totalBytesRecieved = 0;
                sCANFILM.num2Ptr = &num2;
                sCANFILM.digitizerInfoPtr = &digitizerInfo;
                sCANFILM.imageBufferPtr = &imageBuffer;
                sCANFILM.totalBytesRecieved = &totalBytesRecieved;

                fixed (_SCANPARAMETERS* sp_ptr = &digitizeEngine.scan_parameters)
                {
                    sCANFILM.scanParametersPtr = sp_ptr;
                }


                uint status = Scan(sCANFILM.digitizerInfoPtr, sCANFILM.scanParametersPtr, sCANFILM.imageBufferPtr, sCANFILM.totalBytesRecieved);

                Console.WriteLine(status);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }


            return 0;
        }
    }
}
