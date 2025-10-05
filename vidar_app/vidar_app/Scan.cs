// ImageSharp for proper 16-bit grayscale support
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using static vidar_app.Scanner;
using static vidar_app.TiffHandling;
using static vidar_app.VscsiMethods;
using static vidar_app.VscsiTypes;

namespace vidar_app
{
    internal class Scan
    {
        public unsafe static int scan(_DIGITIZERINFO digitizerInfo, ScannerData scanner_data, ScanConfig config)
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();

            try
            {
                digitizeEngine = digitizeEngine2;

                _SCANPARAMETERS scan_parameters = digitizeEngine.InitScanParams(config);

                Console.WriteLine($"Scan parameters loaded from config: {config.Offset2_DPI_X} DPI, {config.Offset0_BitDepth}-bit depth");

                // Guess, this multiplies DPI and max width to get the width of the image.
                scan_parameters.setShort(VscsiTypes._SCANPARAMETERS.OFFSET_Width, (short)(scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_DPI_X) * scanner_data.maxWidthInInches)); // 1050

                // Unsure what the Max_inches value is, but I calculated it to be 51 with the default values.
                // TODO figure out Max_Inches
                scan_parameters.setInt(VscsiTypes._SCANPARAMETERS.OFFSET_Height, scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_DPI_Y) * 51); //?Max_Inches?) //3825

                // To be honest not sure what this one does, but once again matches with the hardcoded defaults.
                scan_parameters.setInt(VscsiTypes._SCANPARAMETERS.OFFSET_BytesPerPixel, (short)Math.Ceiling((double)scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_BitDepth)*0.125)); //1

                // Not sure what this one does either, i think it turns into scanByteCount though.
                //scan_parameters.Field52 = 0;



                uint totalBytesRecieved = 0;
                int status = -1;


                // Taken from decomp
                int imageBufferSize = scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_Width) *
                                    scan_parameters.getInt(VscsiTypes._SCANPARAMETERS.OFFSET_Height) *
                                    scan_parameters.getInt(VscsiTypes._SCANPARAMETERS.OFFSET_BytesPerPixel);


                IntPtr imageBufferPtr = Marshal.AllocHGlobal(imageBufferSize);
                byte* bytePtr = (byte*)imageBufferPtr.ToPointer();

                DigitizeEngine.StartScan(ref status, ref digitizerInfo, ref scan_parameters, ref bytePtr, ref totalBytesRecieved);

                byte[] imageBuffer = new byte[totalBytesRecieved];
                Marshal.Copy(imageBufferPtr, imageBuffer, 0, (int)totalBytesRecieved);
                Marshal.FreeHGlobal(imageBufferPtr);


                // If there is an error while scanning.
                if (status != 0)
                {
                    ushort num3 = 0;
                    _VIDARERRORINFO errInfo = new _VIDARERRORINFO();
                    short s = getVidarError(status, ref errInfo, ref num3);

                    // Print error code and message using new struct properties
                    Console.WriteLine($"ERROR {errInfo.errorCode}: [{errInfo.errorMsg}]");
                    Console.WriteLine();

                    return status;
                }

                // taken from the decompiled code.
                if ((int)(scan_parameters.Data[36]) == 1)
                {
                    // TODO There are some changes here for different options.
                    // i.e. WhiteIsZero, b12BitHigh, and some scan params.
                }


                // Build output filename using config
                string format = (config.OutputFormat ?? "TIFF").ToUpperInvariant();
                string outDir = string.IsNullOrWhiteSpace(config.OutputPath) ? "." : config.OutputPath;

                // Expand environment variables (e.g. %USERPROFILE%) and support ~ for home directory
                outDir = Environment.ExpandEnvironmentVariables(outDir);
                if (outDir.StartsWith("~"))
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string rest = outDir.Length == 1 ? string.Empty : outDir.Substring(1).TrimStart('\\', '/');
                    outDir = Path.Combine(home, rest);
                }
                outDir = Path.GetFullPath(outDir);

                Directory.CreateDirectory(outDir);

                string prefix = config.OutputPrefix ?? "${DPI}DPI_${BIT}BIT";
                prefix = prefix.Replace("${DPI}", scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_DPI_X).ToString());
                prefix = prefix.Replace("${BIT}", scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_BitDepth).ToString());

                string extension = format == "PNG" ? "png" : "tif";
                string fileName = prefix + "." + extension;
                string filePath = Path.Combine(outDir, fileName);

                short width = scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_OutputWidth);
                short height = scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_OutputHeight);

                // Read bit depth from scan parameters (offset 0 is used in the rest of the code)
                short bitDepth = scan_parameters.getShort(VscsiTypes._SCANPARAMETERS.OFFSET_BitDepth);

                // Save based on chosen format
                if (format == "PNG")
                {
                    writeImageToFile(imageBuffer, height, width, filePath, bitDepth);
                }
                else
                {
                    // Prefer existing TIFF writer; writeToTifFile expects scan parameters
                    // If you want to write multi-page TIFFs or implement 16-bit TIFF encoding through ImageSharp,
                    // update writeToTifFile accordingly.
                    int tiffStatus = writeToTifFile(imageBuffer, scan_parameters);
                    Console.WriteLine($"TIFF write status: {tiffStatus}");
                }

                return 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }

        public unsafe static void writeImageToFile(byte[] imageBuffer, int height, int width, string filePath, int bitDepth)
        {
            // Ensure width/height positive
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive values.");

            // Determine whether data is 8-bit or 16-bit per sample
            bool is16Bit = bitDepth > 8;

            try
            {
                if (is16Bit)
                {
                    // Expect two bytes per pixel
                    int expectedBytes = width * height * 2;
                    if (imageBuffer.Length < expectedBytes)
                        throw new ArgumentException($"Image buffer length ({imageBuffer.Length}) is smaller than expected ({expectedBytes}) for 16-bit image.");

                    // Cast the byte span to L16 pixel span (ImageSharp's L16 is a 2-byte grayscale pixel)
                    var spanBytes = imageBuffer.AsSpan(0, expectedBytes);
                    var pixelSpan = MemoryMarshal.Cast<byte, L16>(spanBytes);

                    using (Image<L16> image = Image.LoadPixelData<L16>(pixelSpan, width, height))
                    {
                        var encoder = new PngEncoder
                        {
                            ColorType = PngColorType.Grayscale,
                            BitDepth = PngBitDepth.Bit16
                        };

                        image.Save(filePath, encoder);
                    }
                }
                else
                {
                    // 8-bit grayscale
                    int expectedBytes = width * height;
                    if (imageBuffer.Length < expectedBytes)
                        throw new ArgumentException($"Image buffer length ({imageBuffer.Length}) is smaller than expected ({expectedBytes}) for 8-bit image.");

                    var spanBytes = imageBuffer.AsSpan(0, expectedBytes);
                    var pixelSpan = MemoryMarshal.Cast<byte, L8>(spanBytes);

                    using (Image<L8> image = Image.LoadPixelData<L8>(pixelSpan, width, height))
                    {
                        var encoder = new PngEncoder
                        {
                            ColorType = PngColorType.Grayscale,
                            BitDepth = PngBitDepth.Bit8
                        };

                        image.Save(filePath, encoder);
                    }
                }

                Console.WriteLine("Image saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save image: {ex.Message}");
                throw;
            }
        }
    }
}
