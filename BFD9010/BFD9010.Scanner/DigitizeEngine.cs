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

using static BFD9010.Scanner.VscsiTypes;

namespace BFD9010.Scanner
{
    public class DigitizeEngine
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
        public unsafe _SCANPARAMETERS InitScanParams(ScanConfig config)
        {
            _SCANPARAMETERS scan_parameters = new _SCANPARAMETERS();

            // Populate all offsets from config using named offsets
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset0_BitDepth, _SCANPARAMETERS.OFFSET_BitDepth);
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset2_DPI_X, _SCANPARAMETERS.OFFSET_DPI_X);
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset4_Width, _SCANPARAMETERS.OFFSET_Width);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset8_Height, _SCANPARAMETERS.OFFSET_Height);
            DigitizeEngine.InsertSByte(ref scan_parameters, config.Offset12_Unknown, 12);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset16_Unknown, 16);
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset20_Unknown, 20);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset24_BytesPerPixel, _SCANPARAMETERS.OFFSET_BytesPerPixel);
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset28_Unknown, 28);
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset30_Unknown, 30);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset32_Unknown, 32);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset36_Unknown, 36);
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset40_OutputWidth, _SCANPARAMETERS.OFFSET_OutputWidth);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset44_OutputHeight, _SCANPARAMETERS.OFFSET_OutputHeight);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset48_Unknown, 48);
            // Ensure secondary DPI is set to the same value (ScanConfig.Load already kept them in sync)
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset56_DPI_Y, _SCANPARAMETERS.OFFSET_DPI_Y);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset60_Unknown, 60);
            DigitizeEngine.InsertInt(ref scan_parameters, config.Offset64_Unknown, 64);
            DigitizeEngine.InsertShort(ref scan_parameters, config.Offset68_Unknown, 68);

            return scan_parameters;
        }

        static unsafe void InsertShort(ref _SCANPARAMETERS scanParams, short value, int offset)
        {
            fixed (byte* ptr = scanParams.Data)
            {
                *(short*)(ptr + offset) = value;
            }
        }

        // Method to insert an int at a specific offset in the byte array
        static unsafe void InsertInt(ref _SCANPARAMETERS scanParams, int value, int offset)
        {
            fixed (byte* ptr = scanParams.Data)
            {
                *(int*)(ptr + offset) = value;
            }
        }

        static unsafe void InsertSByte(ref _SCANPARAMETERS scanParams, sbyte value, int offset)
        {
            fixed (byte* ptr = scanParams.Data)
            {
                ptr[offset] = (byte)value;
            }
        }
    }

}
