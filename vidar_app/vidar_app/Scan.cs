using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static vidar_app.VscsiTypes;
using static vidar_app.Scanner;
using static vidar_app.VscsiMethods;
using static vidar_app.TiffHandling;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System;
using System.IO;

namespace vidar_app
{
    internal class Scan
    {
        public unsafe static int scan(_DIGITIZERINFO digitizerInfo, ScannerData scanner_data, ScanConfig config)
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();

            try
            {
                digitizeEngine = digitizeEngine2;

                _SCANPARAMETERS scan_parameters = digitizeEngine.InitScanParams(config);

                Console.WriteLine($"Scan parameters loaded from config: {config.Offset2_DPI} DPI, {config.Offset0_BitDepth}-bit depth");

                // Guess, this multiplies DPI and max width to get the width of the image.
                scan_parameters.setShort(4, (short)(scan_parameters.getShort(2) * scanner_data.maxWidthInInches)); // 1050

                // Unsure what the Max_inches value is, but I calculated it to be 51 with the default values.
                // TODO figure out Max_Inches
                scan_parameters.setInt(8, scan_parameters.getShort(56) * 51); //?Max_Inches?) //3825

                // To be honest not sure what this one does, but once again matches with the hardcoded defaults.
                scan_parameters.setInt(24, (short)Math.Ceiling((double)scan_parameters.getShort(0)*0.125)); //1

                // Not sure what this one does either, i think it turns into scanByteCount though.
                //scan_parameters.Field52 = 0;



                uint totalBytesRecieved = 0;
                int status = -1;


                // Taken from decomp
                int imageBufferSize = scan_parameters.getShort(4) *
                                    scan_parameters.getInt(8) *
                                    scan_parameters.getInt(24);


                IntPtr imageBufferPtr = Marshal.AllocHGlobal(imageBufferSize);
                byte* bytePtr = (byte*)imageBufferPtr.ToPointer();

                DigitizeEngine.StartScan(ref status, ref digitizerInfo, ref scan_parameters, ref bytePtr, ref totalBytesRecieved);

                byte[] imageBuffer = new byte[totalBytesRecieved];
                Marshal.Copy(imageBufferPtr, imageBuffer, 0, (int)totalBytesRecieved);
                Marshal.FreeHGlobal(imageBufferPtr);


                // If there is an error while scanning.
                if (status != 0)
                {
                    ushort num3 = 0;
                    _VIDARERRORINFO errInfo = new _VIDARERRORINFO();
                    short s = getVidarError(status, ref errInfo, ref num3);

                    // Print error code and message using new struct properties
                    Console.WriteLine($"ERROR {errInfo.errorCode}: [{errInfo.errorMsg}]");
                    Console.WriteLine();

                    return status;
                }

                // taken from the decompiled code.
                if ((int)(scan_parameters.Data[36]) == 1)
                {
                    // TODO There are some changes here for different options.
                    // i.e. WhiteIsZero, b12BitHigh, and some scan params.
                }


                //
                Console.WriteLine("File Path to write the scan to (end with .png): ");
                string filePath = Console.ReadLine();

                short width = scan_parameters.getShort(40);
                short height = scan_parameters.getShort(44);
                writeImageToFile(imageBuffer, height, width, filePath);

                //int writingStatus = writeToTifFile(imageBuffer, scan_parameters);


                return 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }

        public unsafe static void writeImageToFile(byte[] imageBuffer, int height, int width, string filePath)
        {
            using (MemoryStream ms = new MemoryStream(imageBuffer))
            {
                using (Bitmap bitmap = new Bitmap(width, height))
                {
                    // Create a grayscale image from the byte array
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // Calculate the byte index based on the position
                            byte grayValue = imageBuffer[y * width + x];
                            Color color = Color.FromArgb(grayValue, grayValue, grayValue);
                            bitmap.SetPixel(x, y, color);
                        }
                    }

                    // Save the image to a file
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }

            Console.WriteLine("Image saved successfully!");
        }
    }
}
