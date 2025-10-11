using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BFD9010.Scanner
{
    public class Scanner
    {
        public struct ScannerData
        {
            public string modelName;
            public string serialNumber;
            public string firmwareVersionNumber;
            public int hardwareVersionNumber;

            public int currentResolution;
            public short opticalResolution;
            public float maxWidthInInches;

            public short currentBitDepth;
            public short maxFilms;

            public string darkEnhance;
            public string lineFilter;
            public string filmBackup;
            public string unloadMedium;
            public string limitedScans;

            public string lineTime;
            public string feederType;
            public string lampType;
            public string translationTable;
        }
    }
}
