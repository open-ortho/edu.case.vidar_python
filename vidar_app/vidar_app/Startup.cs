using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static vidar_app.VscsiTypes;
using static vidar_app.VscsiMethods;
using System.Runtime.CompilerServices;

namespace vidar_app
{
    internal class Startup
    {
        public static unsafe void InitializeDigitizer()
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();
            _DIGITIZERINFO dIGITIZERINFO = new _DIGITIZERINFO();

            try
            {
                digitizeEngine = digitizeEngine2;

                int num = 0;
                ushort num2 = 0;
                ushort num3 = 0;

                // Initialize scan parameters
                digitizeEngine.InitScanParams();

                VscsiTypes.ArrayType1 arr1 = new VscsiTypes.ArrayType1();

                // Initialize HARDWAREINFO structure
                _HARDWAREINFO hardwareInfo = new _HARDWAREINFO
                {
                    Field1 = Marshal.UnsafeAddrOfPinnedArrayElement(new[] { arr1 }, 0),
                    Field2 = Marshal.UnsafeAddrOfPinnedArrayElement(new ushort[] { num2 }, 0),
                    Field3 = Marshal.UnsafeAddrOfPinnedArrayElement(new ushort[] { num3 }, 0)
                };

                StringBuilder stringBuilder = new StringBuilder(4096);
                stringBuilder.Append("Digitizer is warming up, please wait...");

                // Call LocateHardware (placeholder implementation)
                int num4 = (int)Hardware.LocateHardware(hardwareInfo);
                if (num4 == 0)
                {
                    Console.WriteLine("Hardware Located");
                }

                int status = digitizeEngine.GetDigInfo(ref dIGITIZERINFO);
                if (status == 0)
                {
                    Console.WriteLine("Digitizer Info Retrieved");
                }

                Console.WriteLine(getModelName(ref dIGITIZERINFO));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string getModelName(ref _DIGITIZERINFO digitizerInfo)
        {

            // From decompiled code, 104 is the memory offset for the scanner type.
            ushort result = (ushort)digitizerInfo.Data[104];
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
