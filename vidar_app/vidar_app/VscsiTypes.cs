using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace vidar_app
{
    public class VscsiTypes
    {
        [StructLayout(LayoutKind.Sequential, Size = 144)]
        public struct _DIGITIZERINFO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 144)]
            public byte[] Data;
        }




        [StructLayout(LayoutKind.Sequential, Size = 148)]
        public struct _GETDIGINFO
        {
            public int Status;
            public byte[] Data;
        }

        // Method to convert _GETDIGINFO to _DIGITIZERINFO
        // a _GETDIGINFO is  a _DIGITIZERINFO but with a integer prepended
        public static int ConvertGetDigInfoToDigitizerInfo(_GETDIGINFO getDigInfo, out _DIGITIZERINFO digitizerInfo)
        {
            // Extract the 4-byte integer from the _GETDIGINFO struct (Status)
            int status = getDigInfo.Status;

            // Initialize the _DIGITIZERINFO structure
            digitizerInfo = new _DIGITIZERINFO();

            // Copy the remaining bytes into _DIGITIZERINFO (assuming the size of _DIGITIZERINFO is 144 bytes)
            digitizerInfo.Data = new byte[144];  // Assuming _DIGITIZERINFO has a Data array for simplicity
            Array.Copy(getDigInfo.Data, 0, digitizerInfo.Data, 0, 144);  // Copy data starting from byte 4 of _GETDIGINFO

            // Return the extracted status code
            return status;
        }

        [StructLayout(LayoutKind.Explicit, Size = 12)] // Adjust size based on the memory layout
        public struct _HARDWAREINFO
        {
            [FieldOffset(0)] public IntPtr Field1;
            [FieldOffset(4)] public IntPtr Field2; // Pointer to ushort num2
            [FieldOffset(8)] public IntPtr Field3; // Pointer to ushort num3
        }

        [StructLayout(LayoutKind.Sequential, Size = 17)] // Size matches the original definition
        [UnsafeValueType] // Indicates it can be used with pointers
        public struct ArrayType1
        {
            // Add fields here if necessary. 
            // Since you did not specify the fields, we assume it's a raw byte array representation
        }


        [StructLayout(LayoutKind.Sequential, Size = 72)]
        public struct _SCANPARAMETERS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 72)]
            public byte[] Data;
        }

        [StructLayout(LayoutKind.Sequential, Size = 20)]
        public unsafe struct _SCANFILM
        {
            public int* num2Ptr;
            public byte** imageBufferPtr;
            public uint* totalBytesRecieved;
            public _SCANPARAMETERS* scanParametersPtr;
            public _DIGITIZERINFO* digitizerInfoPtr;
        }

        [StructLayout(LayoutKind.Sequential, Size = 82)]
        public unsafe struct _VIDARERRORINFO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 500)]
            public byte[] Data;
        }

    }
}
