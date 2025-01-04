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
        public int ConvertGetDigInfoToDigitizerInfo(_GETDIGINFO getDigInfo, out _DIGITIZERINFO digitizerInfo)
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


        [StructLayout(LayoutKind.Explicit, Size = 72)]
        public struct _SCANPARAMETERS
        {
            // Explicit memory layout with placeholder fields.
            [FieldOffset(0)] public short Field0;     // Offset 0
            [FieldOffset(2)] public short Field2;     // Offset 2
            [FieldOffset(4)] public short Field4;     // Offset 4
            [FieldOffset(8)] public int Field8;       // Offset 8
            [FieldOffset(12)] public sbyte Field12;   // Offset 12
            [FieldOffset(16)] public int Field16;     // Offset 16
            [FieldOffset(20)] public short Field20;   // Offset 20
            [FieldOffset(24)] public int Field24;     // Offset 24
            [FieldOffset(28)] public short Field28;   // Offset 28
            [FieldOffset(30)] public short Field30;   // Offset 30
            [FieldOffset(32)] public int Field32;     // Offset 32
            [FieldOffset(36)] public int Field36;     // Offset 36
            [FieldOffset(40)] public short Field40;   // Offset 40
            [FieldOffset(44)] public int Field44;     // Offset 44
            [FieldOffset(48)] public int Field48;     // Offset 48
            [FieldOffset(56)] public short Field56;   // Offset 56
            [FieldOffset(60)] public int Field60;     // Offset 60
            [FieldOffset(64)] public int Field64;     // Offset 64
            [FieldOffset(68)] public short Field68;   // Offset 68
        }
    }
}
