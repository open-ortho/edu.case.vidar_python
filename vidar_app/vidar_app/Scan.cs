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
        public unsafe static int scan(_DIGITIZERINFO digitizerInfo, ScannerData scanner_data)
        {
            return scan(digitizerInfo, scanner_data, null, null);
        }

        public unsafe static int scan(_DIGITIZERINFO digitizerInfo, ScannerData scanner_data, int? customDpi, int? customBitDepth)
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();

            try
            {
                digitizeEngine = digitizeEngine2;

                _SCANPARAMETERS scan_parameters;
                
                if (customDpi.HasValue && customBitDepth.HasValue)
                {
                    Console.WriteLine($"Using custom scan parameters: {customDpi.Value} DPI, {customBitDepth.Value}-bit depth");
                    scan_parameters = digitizeEngine.InitScanParams(customDpi.Value, customBitDepth.Value);
                }
                else
                {
                    Console.WriteLine("Using default scan parameters: 300 DPI, 16-bit depth");
                    scan_parameters = digitizeEngine.InitScanParams();
                }

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

        // Scan with custom parameters - prompts user for DPI and bit depth
        public unsafe static int scanWithCustomParams(_DIGITIZERINFO digitizerInfo, ScannerData scanner_data)
        {
            Console.WriteLine("\n=== Custom Scan Configuration ===");
            Console.WriteLine("Enter scan parameters (or press Enter to use defaults):");
            
            Console.Write("DPI (default 300): ");
            string dpiInput = Console.ReadLine();
            int dpi = string.IsNullOrWhiteSpace(dpiInput) ? 300 : int.Parse(dpiInput);
            
            Console.Write("Bit Depth - 8 or 16 (default 16): ");
            string bitDepthInput = Console.ReadLine();
            int bitDepth = string.IsNullOrWhiteSpace(bitDepthInput) ? 16 : int.Parse(bitDepthInput);

            return scan(digitizerInfo, scanner_data, dpi, bitDepth);
        }

        // Print the raw scan parameters for debugging
        public static void printScanParameters(_SCANPARAMETERS scan_parameters)
        {
            Console.WriteLine("\n=== Raw Scan Parameters (72 bytes) ===");
            Console.WriteLine("Offset | Value (bytes) | Value (interpreted)");
            Console.WriteLine("-------|---------------|--------------------");
            
            // Key parameters we know
            Console.WriteLine($"  0    | {scan_parameters.getShort(0):D5}        | Bit depth");
            Console.WriteLine($"  2    | {scan_parameters.getShort(2):D5}        | DPI (primary)");
            Console.WriteLine($"  4    | {scan_parameters.getShort(4):D5}        | Width in pixels");
            Console.WriteLine($"  8    | {scan_parameters.getInt(8):D5}        | Height in pixels");
            Console.WriteLine($" 12    | {scan_parameters.Data[12]:D5}        | Unknown byte");
            Console.WriteLine($" 16    | {scan_parameters.getInt(16):D5}        | Unknown int");
            Console.WriteLine($" 20    | {scan_parameters.getShort(20):D5}        | Unknown short");
            Console.WriteLine($" 24    | {scan_parameters.getInt(24):D5}        | Bytes per pixel");
            Console.WriteLine($" 28    | {scan_parameters.getShort(28):D5}        | Unknown short");
            Console.WriteLine($" 30    | {scan_parameters.getShort(30):D5}        | Unknown short");
            Console.WriteLine($" 32    | {scan_parameters.getInt(32):D5}        | Unknown int");
            Console.WriteLine($" 36    | {scan_parameters.getInt(36):D5}        | Unknown int");
            Console.WriteLine($" 40    | {scan_parameters.getShort(40):D5}        | Output width");
            Console.WriteLine($" 44    | {scan_parameters.getInt(44):D5}        | Output height");
            Console.WriteLine($" 48    | {scan_parameters.getInt(48):D5}        | Unknown int");
            Console.WriteLine($" 56    | {scan_parameters.getShort(56):D5}        | DPI (secondary)");
            Console.WriteLine($" 60    | {scan_parameters.getInt(60):D5}        | Unknown int");
            Console.WriteLine($" 64    | {scan_parameters.getInt(64):D5}        | Unknown int");
            Console.WriteLine($" 68    | {scan_parameters.getShort(68):D5}        | Unknown short");
            
            Console.WriteLine("\nFull hex dump:");
            for (int i = 0; i < scan_parameters.Data.Length; i += 16)
            {
                Console.Write($"{i:D3}: ");
                for (int j = 0; j < 16 && (i + j) < scan_parameters.Data.Length; j++)
                {
                    Console.Write($"{scan_parameters.Data[i + j]:X2} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("=====================================\n");
        }
    }
}
