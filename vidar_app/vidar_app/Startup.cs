/*
 * Startup.cs
 * Scanner initialization and digitizer information retrieval.
 *
 * Responsibilities:
 *  - Initialize and detect connected Vidar scanner hardware
 *  - Retrieve and parse digitizer capabilities and configuration
 *  - Display scanner model, firmware version, and supported features
 *
 * Target framework: .NET 8
 */

using System.Runtime.InteropServices;
using System.Text;
using static vidar_app.VscsiTypes;
using static vidar_app.Scanner;

namespace vidar_app
{
    internal class Startup
    {
        public static unsafe int InitializeDigitizer(ref _DIGITIZERINFO dIGITIZERINFO, ref ScannerData scanner_data)
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();

            try
            {
                digitizeEngine = digitizeEngine2;

                int num = 0;
                ushort num2 = 0;
                ushort num3 = 0;

                VscsiTypes.ArrayType1 arr1 = new VscsiTypes.ArrayType1();

                // Initialize HARDWAREINFO structure
                _HARDWAREINFO hardwareInfo = new _HARDWAREINFO
                {
                    Field1 = Marshal.UnsafeAddrOfPinnedArrayElement(new[] { arr1 }, 0),
                    Field2 = Marshal.UnsafeAddrOfPinnedArrayElement(new ushort[] { num2 }, 0),
                    Field3 = Marshal.UnsafeAddrOfPinnedArrayElement(new ushort[] { num3 }, 0)
                };

                StringBuilder stringBuilder = new StringBuilder(4096);
                // Call LocateHardware (placeholder implementation)
                int num4 = (int)Hardware.LocateHardware(hardwareInfo);
                if (num4 == 0)
                {
                } else
                {
                    Console.WriteLine($"Hardware could not be found with error code: {num4}");
                    return num4;
                }


                // Get the digitizer's parameters and print them to console.
                Console.WriteLine("Reading Digitizer Capabilities...");

                int status = digitizeEngine.GetDigInfo(ref dIGITIZERINFO);
                if (status == 0)
                {
                    Console.WriteLine("Digitizer Info Retrieved");
                    parseScannerInfo(ref dIGITIZERINFO, ref scanner_data);
                    return 0;
                } else
                {
                    Console.WriteLine($"Digitizer Info could not be Retrieved with error code: {status}");
                    return status;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static unsafe void parseScannerInfo(ref _DIGITIZERINFO digitizerInfo, ref ScannerData scanner_data)
        {
            // TODO Go through the loops in the decomp for the drop downs.

            // Offsets are from decompiled code, size and data type are inferred through trial and error.
            scanner_data.modelName = getModelName(ref digitizerInfo);
            scanner_data.serialNumber = parseStringValue(ref digitizerInfo, 106, 6);
            scanner_data.firmwareVersionNumber = parseStringValue(ref digitizerInfo, 113, 4);
            
            fixed (byte* ptr = digitizerInfo.Data)
            {
                scanner_data.hardwareVersionNumber = (int)ptr[118];
                scanner_data.currentResolution = (int)ptr[42];
            }

            scanner_data.opticalResolution = parseShortValue(ref digitizerInfo, 44);
            scanner_data.maxWidthInInches = parseFloatValue(ref digitizerInfo, 72, 10);

            scanner_data.currentBitDepth = parseShortValue(ref digitizerInfo, 68);
            scanner_data.maxFilms = parseShortValue(ref digitizerInfo, 90);

            scanner_data.darkEnhance = parseBinaryValue(ref digitizerInfo, 86, "Dark Enhance not available", "Dark Enhance available");
            scanner_data.lineFilter = parseBinaryValue(ref digitizerInfo, 88, "Line Filter not available", "Line Filter available");
            scanner_data.filmBackup = parseBinaryValue(ref digitizerInfo, 92, "Film backup not available", "Film backup available");
            scanner_data.unloadMedium = parseBinaryValue(ref digitizerInfo, 100, "Single unloadMedium() command to eject film", "Double unloadMedium() command to eject film");
            scanner_data.limitedScans = parseBinaryValue(ref digitizerInfo, 140, "Unlimited scans device", "Limited scans device");

            scanner_data.lineTime = getLineTime(ref digitizerInfo);
            scanner_data.feederType = getFeederType(ref digitizerInfo);
            scanner_data.lampType = getLampType(ref digitizerInfo);
            scanner_data.translationTable = getTranslationTable(ref digitizerInfo);
            // time since reset is stored here as well..


            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("-------------------------------------------------------------");

            Console.WriteLine($"Digitizer model: {scanner_data.modelName}");
            Console.WriteLine($"Serial Number: {scanner_data.serialNumber}");
            Console.WriteLine($"Firmware version number: {scanner_data.firmwareVersionNumber}");
            Console.WriteLine($"Hardware version number: {scanner_data.hardwareVersionNumber}");

            Console.WriteLine($"Current resolution = {scanner_data.currentResolution}");
            Console.WriteLine($"Optical resolution = {scanner_data.opticalResolution}");
            Console.WriteLine($"Maximum width in inches = {scanner_data.maxWidthInInches}");

            Console.WriteLine($"Current Bit Depth = {scanner_data.currentBitDepth}");
            Console.WriteLine($"Maximum number of films = {scanner_data.maxFilms}");

            Console.WriteLine(scanner_data.darkEnhance);
            Console.WriteLine(scanner_data.lineFilter);
            Console.WriteLine(scanner_data.filmBackup);
            Console.WriteLine(scanner_data.unloadMedium);
            Console.WriteLine(scanner_data.limitedScans);

            Console.WriteLine(scanner_data.lineTime);
            Console.WriteLine(scanner_data.feederType);
            Console.WriteLine(scanner_data.lampType);
            Console.Write(scanner_data.translationTable);

            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("");
            Console.WriteLine("");

        }

        public static unsafe string parseStringValue(ref _DIGITIZERINFO digitizerInfo, int offset, int size)
        {
            byte[] extractedBytes = new byte[size];
            fixed (byte* ptr = digitizerInfo.Data)
            {
                for (int i = 0; i < size; i++)
                {
                    extractedBytes[i] = ptr[offset + i];
                }
            }

            return Encoding.ASCII.GetString(extractedBytes);
        }

        public static unsafe float parseFloatValue(ref _DIGITIZERINFO digitizerInfo, int offset, int size)
        {
            byte[] extractedBytes = new byte[size];
            fixed (byte* ptr = digitizerInfo.Data)
            {
                for (int i = 0; i < size; i++)
                {
                    extractedBytes[i] = ptr[offset + i];
                }
            }

            return BitConverter.ToSingle(extractedBytes, 0);
        }

        public static unsafe short parseShortValue(ref _DIGITIZERINFO digitizerInfo, int offset)
        {
            fixed (byte* ptr = digitizerInfo.Data)
            {
                return *(short*)(ptr + offset);
            }
        }

        public static unsafe string parseBinaryValue(ref _DIGITIZERINFO digitizerInfo, int offset, string ifZero, string ifOne)
        {
            short value;
            fixed (byte* ptr = digitizerInfo.Data)
            {
                value = *(short*)(ptr + offset);
            }

            if (value == 0)
            {
                return ifZero;
            } else
            {
                return ifOne;
            }
        }

        public static unsafe string getLineTime(ref _DIGITIZERINFO digitizerInfo)
        {
            short value = parseShortValue(ref digitizerInfo, 84);

            StringBuilder sb = new StringBuilder("");

            if (value == 1)
            {
                short fixedLineTime = parseShortValue(ref digitizerInfo, 82);

                sb.Append($"Fixed Line = {fixedLineTime}");
            } else
            {
                short lineTimeRangeStart = parseShortValue(ref digitizerInfo, 78);
                short lineTimeRangeEnd = parseShortValue(ref digitizerInfo, 80);

                // this below value could be a int instead of a short but I have no way to test without the hardware.
                short lineTimeCurrent = parseShortValue(ref digitizerInfo, 82);

                sb.AppendLine($"Line time range start = {lineTimeRangeStart}");
                sb.AppendLine($"Line time range end = {lineTimeRangeEnd}");
                sb.AppendLine($"Line time current = {lineTimeCurrent}");

            }

            return sb.ToString();
        }

        public static unsafe string getTranslationTable(ref _DIGITIZERINFO digitizerInfo)
        {
            short value = parseShortValue(ref digitizerInfo, 96);

            StringBuilder sb = new StringBuilder("");

            ushort scannerType;
            fixed (byte* ptr = digitizerInfo.Data)
            {
                scannerType = ptr[104];
            }

            if (value == 1)
            {
                sb.AppendLine("Translation table selection available.");
            } else if (value == 0)
            {
                if (scannerType != 19 && scannerType != 22 && scannerType != 23)
                {
                    sb.AppendLine("Translation tables are limited to Linear and LOG.");
                } else
                {
                    sb.AppendLine("Translation table selection is limited to Power 5 only.");
                }
            }

            short value2 = parseShortValue(ref digitizerInfo, 98);

            switch (value2)
            {
                default:
                    sb.AppendLine("Translation table locations 0 and 1 setting unknown.");
                    break;
                case 5:
                    sb.AppendLine("Translation table locations 0 and 1 set to Power 5.");
                    break;
                case 1:
                    if (scannerType != 19 && scannerType != 22 && scannerType != 23)
                    {
                        sb.AppendLine("Translation table locations 0 and 1 set to LOG..");
                    }
                    else
                    {
                        sb.AppendLine("Translation table locations 0 and 1 set to Power 5.");
                    }

                    break;
                case 0:
                    sb.AppendLine("Translation table location 0 set to Linear, location 1 set to LOG.");
                    break;

            }

            return sb.ToString();
        }

        public static unsafe string getFeederType(ref _DIGITIZERINFO digitizerInfo)
        {
            int value = parseShortValue(ref digitizerInfo, 102);
            
            switch (value)
            {
                case 3:
                    return "Feeder type is B (9.45 inches wide, 100 sheet capacity).";
                case 2:
                    return "Feeder type is C (14 inches wide).";
                case 4:
                    return "Feeder type is D (Smartfeeder XL, 9.45 to 10 inches wide with loading flap).";
                case 0:
                    return "No feeder found.";
                case 253:
                    return "Feeder type is A3 (9.45 inches wide, no loading flap).";
                case 255:
                    return "Feeder type is Film Director.";
                case 254:
                    return "Feeder type is A2 (9.45 inches wide with loading flap).";
                default:
                    return "Feeder type is unknown.";
            }

        }

        public static unsafe string getLampType(ref _DIGITIZERINFO digitizerInfo)
        {
            int value = parseShortValue(ref digitizerInfo, 138);

            switch (value)
            {
                case 0:
                    return "Lamp type is a fluorescent bulb.";
                case 7:
                    return "Lamp type is a fluorescent bulb full width cartridge.";
                case 8:
                    return "Lamp type is a fluorescent bulb mammo width cartridge.";
                case 9:
                    return "Lamp type is a fluorescent bulb full width roller cartridge.";
                case 10:
                    return "Lamp type is a fluorescent bulb mammo width roller cartridge.";
                case 1:
                    return "Lamp type is a white LED full width cartridge.";
                case 2:
                    return "Lamp type is a red LED full width cartridge.";
                case 3:
                    return "Lamp type is a white LED full width roller cartridge.";
                case 4:
                    return "Lamp type is a red LED full width roller cartridge.";
                case 5:
                    return "Lamp type is a white LED mammo width cartridge.";
                case 6:
                    return "Lamp type is a white LED mammo width roller cartridge.";
                case 11:
                    return "Lamp type is a white LED ten inch width cartridge.";
                case 12:
                    return "Lamp type is a blue LED full width cartridge.";
                case 13:
                    return "Lamp type is a green LED full width cartridge.";
                default:
                    return "Lamp type is unknown.";
            }
        }

        public static unsafe string getModelName(ref _DIGITIZERINFO digitizerInfo)
        {

            // From decompiled code, 104 is the memory offset for the scanner type.
            ushort result;
            fixed (byte* ptr = digitizerInfo.Data)
            {
                result = ptr[104];
            }
            
            string value;

            switch(result) {

                case 1:
                    value = "VXR-12";
                    break;
                case 2:
                    value = "VXR-12 plus";
                    break;
                case 3:
                    value = "VXR-16";
                    break;
                case 4:
                    value = "VXR-16 DosimetryPRO";
                    break;
                case 5:
                    value = "DiagnosticPRO";
                    break;
                case 6:
                    value = "DiagnosticPRO plus (USA model)";
                    break;
                case 7:
                    value = "DiagnosticPRO plus (European model)";
                    break;
                case 8:
                    value = "DiagnosticPRO - M";
                    break;
                case 9:
                    value = "SIERRA";
                    break;
                case 10:
                    value = "SIERRA plus";
                    break;
                case 16:
                case 18:
                    value = "SIERRA Advantage";
                    break;
                case 11:
                    value = "DiagnosticPRO Advantage (USA model)";
                    break;
                case 12:
                    value = "DiagnosticPRO Advantage (European model)";
                    break;
                case 13:
                    value = "DosimetryPRO Advantage";
                    break;
                case 14:
                    value = "CAD PRO Advantage";
                    break;
                case 17:
                    value = "Dental Film Digitizer";
                    break;
                case 19:
                case 20:
                    value = "TeleradPRO";
                    break;
                case 21:
                    value = "VETradPRO";
                    break;
                case 22:
                    value = "TeleradPRO Edge";
                    break;
                case 23:
                    value = "VETradPRO Edge";
                    break;
                case 24:
                    value = "NDTPRO";
                    break;
                default:
                    value = "Unknown model";
                    break;
                }

                return value;
            }
    }

}
