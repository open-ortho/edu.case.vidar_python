using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static vidar_app.Scanner;
using static vidar_app.VscsiTypes;

namespace vidar_app
{
    internal class TiffHandling
    {
        public unsafe static int writeToTifFile(byte[] imageBuffer, _SCANPARAMETERS sp)
        {
            Tiffheader tiffheader = new Tiffheader();

            // .tif magic bytes
            tiffheader.field1 = (short)18761;
            tiffheader.field2 = (short)42;
            tiffheader.field3 = (int)8;//((short)(sp.Data[40]) * (int)(sp.Data[24]) * (int)(sp.Data[44])) + 8;

            string filePath = "output.tif";

            using (FileStream outputFile = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(outputFile))
            {
                byte[] tiffHeaderBytes = tiffheader.toByteArray();
                // Write the TIFF header (8 bytes)
                writer.Write(tiffHeaderBytes, 0, 8);

                // Write the image buffer
                writer.Write(imageBuffer, 0, imageBuffer.Length);

                // add tiff tags
                int tagStatus = createTiffTags(writer, sp);

            }


            return 0;
        }

        public static unsafe int createTiffTags(BinaryWriter writer, _SCANPARAMETERS sp)
        {
            // TODO, NONE OF THE SP CALLS WORK, NEED TO USE .GETSHORT
            // start
            ushort num = 15;
            writer.Write(num);

            // IL_0075
            TiffTag tiffTag = new TiffTag(254, 4, 1, 0);
            writer.Write(tiffTag.toByteArray());

            // IL_00b7
            tiffTag = new TiffTag(256, 4, 1, (int)sp.Data[40]);
            writer.Write(tiffTag.toByteArray());

            // IL_0101
            tiffTag = new TiffTag(257, 4, 1, (int)sp.Data[44]);
            writer.Write(tiffTag.toByteArray());

            // IL_014b
            tiffTag = new TiffTag(258, 3, 1, ((int)sp.Data[24] << 3));
            writer.Write(tiffTag.toByteArray());

            // IL_0197
            tiffTag = new TiffTag(259, 3, 1, 1);
            writer.Write(tiffTag.toByteArray());

            // IL_01d9
            // TODO, if WhiteIsZero is true than last parameter is 0
            tiffTag = new TiffTag(262, 3, 1, 1);
            writer.Write(tiffTag.toByteArray());

            // IL_0226
            tiffTag = new TiffTag(273, 4, 1, 8);
            writer.Write(tiffTag.toByteArray());

            // IL_0268
            tiffTag = new TiffTag(277, 3, 1, 1);
            writer.Write(tiffTag.toByteArray());

            // IL_02aa
            tiffTag = new TiffTag(278, 4, 1, (int)sp.Data[44]);
            writer.Write(tiffTag.toByteArray());

            // IL_02f4
            tiffTag = new TiffTag(279, 4, 1, (ushort)sp.Data[40] * (int)sp.Data[24] * (int)sp.Data[44]);
            writer.Write(tiffTag.toByteArray());

            // IL_0352
            tiffTag = new TiffTag(282, 5, 1, num * 12 + sp.Data[40] * sp.Data[24] * sp.Data[44] + 14);
            writer.Write(tiffTag.toByteArray());

            // IL_03b8
            tiffTag = new TiffTag(283, 5, 1, num * 12 + sp.Data[40] * sp.Data[24] * sp.Data[44] + 22);
            writer.Write(tiffTag.toByteArray());

            // IL_041e
            tiffTag = new TiffTag(296, 3, 1, 2);
            writer.Write(tiffTag.toByteArray());

            // IL_0460
            //tiffTag = new TiffTag(305, 2, num4, num * 12 + sp.Data[40] * sp.Data[24] * sp.Data[44] + 22);

            return 0;
        }
    }
}
