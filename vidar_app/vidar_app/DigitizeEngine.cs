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


        public int GetDigInfo(ref _DIGITIZERINFO digitizerInfo)
        {
            // Call the external method to populate the structure
            int status = VscsiMethods.getDigitizerInfo(ref digitizerInfo);

            // Return the status code
            return status;
        }


        public static unsafe void StartScan(ref int status, ref _DIGITIZERINFO digitizerInfo, ref _SCANPARAMETERS scan_parameters, ref byte* imageBuffer, ref uint totalBytesRecieved)
        {
            status = VscsiMethods.Scan(ref digitizerInfo, ref scan_parameters, ref imageBuffer, ref totalBytesRecieved);
        }
        

        public _SCANPARAMETERS InitScanParams()
        {
            // Use default 300 DPI and 16-bit depth
            return InitScanParams(300, 16);
        }

        public _SCANPARAMETERS InitScanParams(int dpi, int bitDepth)
        {
            _SCANPARAMETERS scan_parameters = new _SCANPARAMETERS();

            scan_parameters.Data = new byte[72];

            // Bit depth at offset 0
            DigitizeEngine.InsertShort(scan_parameters.Data, (short)bitDepth, 0);

            // DPI resolution at offsets 2 and 56
            DigitizeEngine.InsertShort(scan_parameters.Data, (short)dpi, 56);
            DigitizeEngine.InsertShort(scan_parameters.Data, (short)dpi, 2);

            // Width: DPI * 14 inches (approximate max width)
            DigitizeEngine.InsertShort(scan_parameters.Data, (short)(dpi * 14), 4);

            DigitizeEngine.InsertShort(scan_parameters.Data, 1, 28);
            DigitizeEngine.InsertShort(scan_parameters.Data, 0, 30);
            DigitizeEngine.InsertShort(scan_parameters.Data, 1, 20);
            DigitizeEngine.InsertShort(scan_parameters.Data, 8400, 40);
            DigitizeEngine.InsertShort(scan_parameters.Data, 0, 68);

            // Height: DPI * 51 inches (calculated from original ratio)
            DigitizeEngine.InsertInt(scan_parameters.Data, dpi * 51, 8);

            // Bytes per pixel: calculated from bit depth
            DigitizeEngine.InsertInt(scan_parameters.Data, (int)Math.Ceiling((double)bitDepth * 0.125), 24);

            DigitizeEngine.InsertInt(scan_parameters.Data, 1, 32);
            DigitizeEngine.InsertInt(scan_parameters.Data, 1, 36);
            DigitizeEngine.InsertInt(scan_parameters.Data, 1, 16);
            DigitizeEngine.InsertInt(scan_parameters.Data, 30600, 44);
            DigitizeEngine.InsertInt(scan_parameters.Data, 0, 48);
            DigitizeEngine.InsertInt(scan_parameters.Data, 0, 60);
            DigitizeEngine.InsertInt(scan_parameters.Data, 0, 64);

            DigitizeEngine.InsertSByte(scan_parameters.Data, 2, 12);


            return scan_parameters;
        }

        // Initialize scan parameters with raw byte array for advanced users
        public _SCANPARAMETERS InitScanParamsRaw(byte[] rawData)
        {
            _SCANPARAMETERS scan_parameters = new _SCANPARAMETERS();

            if (rawData.Length != 72)
            {
                throw new ArgumentException("Raw scan parameters must be exactly 72 bytes");
            }

            scan_parameters.Data = new byte[72];
            Array.Copy(rawData, scan_parameters.Data, 72);

            return scan_parameters;
        }

        static void InsertShort(byte[] byteArray, short value, int offset)
        {
            byte[] shortBytes = BitConverter.GetBytes(value);

            // Ensure the value fits in the byte array range
            if (offset + shortBytes.Length <= byteArray.Length)
            {
                Array.Copy(shortBytes, 0, byteArray, offset, shortBytes.Length);
            }
            else
            {
                Console.WriteLine("Error: Insertion goes out of bounds for short.");
            }
        }

        // Method to insert an int at a specific offset in the byte array
        static void InsertInt(byte[] byteArray, int value, int offset)
        {
            byte[] intBytes = BitConverter.GetBytes(value);

            // Ensure the value fits in the byte array range
            if (offset + intBytes.Length <= byteArray.Length)
            {
                Array.Copy(intBytes, 0, byteArray, offset, intBytes.Length);
            }
            else
            {
                Console.WriteLine("Error: Insertion goes out of bounds for int.");
            }
        }

        static void InsertSByte(byte[] byteArray, sbyte value, int offset)
        {
            byte[] sbyteBytes = new byte[] { (byte)value };  // Convert sbyte to byte

            // Ensure the value fits in the byte array range
            if (offset + sbyteBytes.Length <= byteArray.Length)
            {
                Array.Copy(sbyteBytes, 0, byteArray, offset, sbyteBytes.Length);
            }
            else
            {
                Console.WriteLine("Error: Insertion goes out of bounds for sbyte.");
            }
        }
    }

}
