using System;
using System.Runtime.InteropServices;
using System.Security;
using static vidar_app.VscsiTypes;

namespace vidar_app
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
    }
}