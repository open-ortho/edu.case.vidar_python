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
            _SCANPARAMETERS scan_parameters = new _SCANPARAMETERS();

            scan_parameters.Data = new byte[72];

            //DigitizeEngine.InsertShort(scan_parameters.Data, 16, 0);
            DigitizeEngine.InsertShort(scan_parameters.Data, 8, 0);


            //DigitizeEngine.InsertShort(scan_parameters.Data, 600, 2);
            //DigitizeEngine.InsertShort(scan_parameters.Data, 600, 56);
            DigitizeEngine.InsertShort(scan_parameters.Data, 75, 56);
            DigitizeEngine.InsertShort(scan_parameters.Data, 75, 2);


            //DigitizeEngine.InsertShort(scan_parameters.Data, 8400, 4);
            DigitizeEngine.InsertShort(scan_parameters.Data, 1050, 4);

            DigitizeEngine.InsertShort(scan_parameters.Data, 1, 28);
            DigitizeEngine.InsertShort(scan_parameters.Data, 0, 30);
            DigitizeEngine.InsertShort(scan_parameters.Data, 1, 20);
            DigitizeEngine.InsertShort(scan_parameters.Data, 8400, 40);
            DigitizeEngine.InsertShort(scan_parameters.Data, 0, 68);

            //DigitizeEngine.InsertInt(scan_parameters.Data, 30600, 8);
            DigitizeEngine.InsertInt(scan_parameters.Data, 3825, 8);

            //DigitizeEngine.InsertInt(scan_parameters.Data, 2, 24);
            DigitizeEngine.InsertInt(scan_parameters.Data, 1, 24);

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
