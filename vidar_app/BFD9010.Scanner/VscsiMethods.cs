using System.Runtime.InteropServices;
using System.Security;
using static BFD9010.Scanner.VscsiTypes;

namespace BFD9010.Scanner
{
    unsafe public static class VscsiMethods
    {
        [DllImport("Vscsi32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getDigitizerInfo(ref _DIGITIZERINFO digitizerInfo);

        [DllImport("Vscsi32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int EjectFilm(ref _DIGITIZERINFO digitizerInfo, int flag);


        [DllImport("Vscsi32.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Calibrate();


        [DllImport("Vscsi32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern int findDigitizer(byte* P_0, ushort* P_1, ushort* P_2);

        [DllImport("Vscsi32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern int Scan(ref _DIGITIZERINFO P_0, ref _SCANPARAMETERS P_1, ref byte* P_2, ref uint P_3);

        [DllImport("Vscsi32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern short getVidarError(int P_0, ref _VIDARERRORINFO P_1, ref ushort P_2);
    }
}