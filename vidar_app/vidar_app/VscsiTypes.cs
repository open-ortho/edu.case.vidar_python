/*
 * VscsiTypes.cs
 * Type definitions for Vidar SCSI/USB scanner communication.
 *
 * Responsibilities:
 *  - Define P/Invoke-compatible structs for scanner data exchange
 *  - Provide byte-array accessors for scan parameters and device info
 *  - Support marshalling between managed and native scanner DLL structures
 *
 * Target framework: .NET 8
 */

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
            // Named offsets for fields in the Data array to improve readability
            public const int OFFSET_BitDepth = 0;           // short
            public const int OFFSET_DPI_X = 2;              // short - X DPI
            public const int OFFSET_Width = 4;              // short
            public const int OFFSET_Height = 8;             // int
            public const int OFFSET_BytesPerPixel = 24;     // int
            public const int OFFSET_OutputWidth = 40;       // short (returned actual width)
            public const int OFFSET_OutputHeight = 44;      // int (returned actual height)
            public const int OFFSET_DPI_Y = 56;             // short - Y DPI (secondary)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 72)]
            public byte[] Data;

            public unsafe short getShort(int offset)
            {
                byte[] shortBytes = new byte[2];
                Array.Copy(this.Data, offset, shortBytes, 0, 2);

                return (short)BitConverter.ToInt16(shortBytes, 0);
            }

            public unsafe int getInt(int offset)
            {
                byte[] intBytes = new byte[4];
                Array.Copy(this.Data, offset, intBytes, 0, 4);

                return BitConverter.ToInt32(intBytes, 0);
            }

            public unsafe void setShort(int offset, short value)
            {
                byte[] shortBytes = BitConverter.GetBytes(value);

                // Set the bytes in the byte array at the specified offset
                Array.Copy(shortBytes, 0, this.Data, offset, 2);
            }

            public unsafe void setInt(int offset, int value)
            {
                byte[] intBytes = BitConverter.GetBytes(value);

                // Set the bytes in the byte array at the specified offset
                Array.Copy(intBytes, 0, this.Data, offset, 4);
            }
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

            public short errorCode => Data != null && Data.Length >= 2 ? BitConverter.ToInt16(Data, 0) : (short)0;
            public string errorMsg => Data != null && Data.Length > 2 ? Encoding.ASCII.GetString(Data, 2, Data.Length - 2).TrimEnd('\0') : string.Empty;
        }
    }
}
