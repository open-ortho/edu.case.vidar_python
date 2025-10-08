using static BFD9010.Scanner.VscsiTypes;

namespace BFD9010.Scanner
{
    public class Hardware
    {
        internal unsafe static uint LocateHardware(VscsiTypes._HARDWAREINFO HwInfo)
        {
            DigitizeEngine digitizeEngine = new DigitizeEngine();
            try
            {
                _HARDWAREINFO* hwInfoPtr = &HwInfo;

                // Access the fields of _HARDWAREINFO
                byte* pNum1 = (byte*)hwInfoPtr->Field1;
                ushort* pNum2 = (ushort*)hwInfoPtr->Field2;
                ushort* pNum3 = (ushort*)hwInfoPtr->Field3;

                DigitizeEngine.ErrorCode = VscsiMethods.findDigitizer(pNum1, pNum2, pNum3);

                //MsvcrtMethods._endthreadex(0u);
            }
            catch
            {
                throw;
            }
            return 0u;
        }
    }
}
