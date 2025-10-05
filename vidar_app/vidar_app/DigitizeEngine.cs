/*
 * DigitizeEngine.cs
 * Core scan parameter initialization and execution engine.
 *
 * Responsibilities:
 *  - Initialize scan parameters from ScanConfig
 *  - Convert managed scan parameters to native format for VSCSI driver
 *  - Coordinate scan execution via VscsiMethods
 *  - Retrieve digitizer information
 *
 * Target framework: .NET 8
 */

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
        

        /// <summary>
        /// Initialize scan parameters from a configuration object.
        /// </summary>
        public _SCANPARAMETERS InitScanParams(ScanConfig config)
        {
            _SCANPARAMETERS scan_parameters = new _SCANPARAMETERS();
            scan_parameters.Data = new byte[72];

            // Populate all offsets from config using named offsets
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset0_BitDepth, _SCANPARAMETERS.OFFSET_BitDepth);
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset2_DPI_X, _SCANPARAMETERS.OFFSET_DPI_X);
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset4_Width, _SCANPARAMETERS.OFFSET_Width);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset8_Height, _SCANPARAMETERS.OFFSET_Height);
            DigitizeEngine.InsertSByte(scan_parameters.Data, config.Offset12_Unknown, 12);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset16_Unknown, 16);
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset20_Unknown, 20);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset24_BytesPerPixel, _SCANPARAMETERS.OFFSET_BytesPerPixel);
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset28_Unknown, 28);
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset30_Unknown, 30);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset32_Unknown, 32);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset36_Unknown, 36);
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset40_OutputWidth, _SCANPARAMETERS.OFFSET_OutputWidth);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset44_OutputHeight, _SCANPARAMETERS.OFFSET_OutputHeight);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset48_Unknown, 48);
            // Ensure secondary DPI is set to the same value (ScanConfig.Load already kept them in sync)
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset56_DPI_Y, _SCANPARAMETERS.OFFSET_DPI_Y);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset60_Unknown, 60);
            DigitizeEngine.InsertInt(scan_parameters.Data, config.Offset64_Unknown, 64);
            DigitizeEngine.InsertShort(scan_parameters.Data, config.Offset68_Unknown, 68);

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
