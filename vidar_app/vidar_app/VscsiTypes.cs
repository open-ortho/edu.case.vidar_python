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
        public unsafe struct _DIGITIZERINFO
        {
            public fixed byte Data[144];
        }

        [StructLayout(LayoutKind.Sequential, Size = 148)]
        public unsafe struct _GETDIGINFO
        {
            public int Status;
            public fixed byte Data[144];
        }

        // Method to convert _GETDIGINFO to _DIGITIZERINFO
        // a _GETDIGINFO is  a _DIGITIZERINFO but with a integer prepended
        public static unsafe int ConvertGetDigInfoToDigitizerInfo(_GETDIGINFO getDigInfo, out _DIGITIZERINFO digitizerInfo)
        {
            // Extract the 4-byte integer from the _GETDIGINFO struct (Status)
            int status = getDigInfo.Status;

            // Initialize the _DIGITIZERINFO structure
            digitizerInfo = new _DIGITIZERINFO();

            // Copy the remaining bytes into _DIGITIZERINFO
            for (int i = 0; i < 144; i++)
            {
                digitizerInfo.Data[i] = getDigInfo.Data[i];
            }

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
        public unsafe struct _SCANPARAMETERS
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

            public fixed byte Data[72];

            public short getShort(int offset)
            {
                fixed (byte* ptr = Data)
                {
                    return *(short*)(ptr + offset);
                }
            }

            public int getInt(int offset)
            {
                fixed (byte* ptr = Data)
                {
                    return *(int*)(ptr + offset);
                }
            }

            public void setShort(int offset, short value)
            {
                fixed (byte* ptr = Data)
                {
                    *(short*)(ptr + offset) = value;
                }
            }

            public void setInt(int offset, int value)
            {
                fixed (byte* ptr = Data)
                {
                    *(int*)(ptr + offset) = value;
                }
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

        // Error information structure returned by the native Vscsi32.dll
        // Note: Original decompiled code had Size = 82, but this was likely an error.
        // The 500-byte buffer is necessary to accommodate full error messages from the scanner hardware.
        // Risk analysis:
        //   - Size too small (82): Buffer overrun when DLL writes long error messages → crash/corruption
        //   - Size too large (500): Wastes stack space but prevents corruption → safe
        // The native DLL will write based on its own struct definition, so we must provide adequate space.
        [StructLayout(LayoutKind.Sequential, Size = 500)]
        public unsafe struct _VIDARERRORINFO
        {
            public fixed byte Data[500];

            public short errorCode
            {
                get
                {
                    fixed (byte* ptr = Data)
                    {
                        return *(short*)ptr;
                    }
                }
            }

            public string errorMsg
            {
                get
                {
                    fixed (byte* ptr = Data)
                    {
                        // Skip first 2 bytes (errorCode) and read the rest as ASCII string
                        int length = 0;
                        for (int i = 2; i < 500 && ptr[i] != 0; i++)
                        {
                            length++;
                        }
                        return Encoding.ASCII.GetString(ptr + 2, length);
                    }
                }
            }
        }
    }
}
