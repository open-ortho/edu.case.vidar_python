using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static vidar_app.VscsiTypes;

namespace vidar_app
{
    internal class Hardware
    {
        internal unsafe static uint LocateHardware(VscsiTypes._HARDWAREINFO HwInfo)
        {
            DigitizeEngine digitizeEngine = null;
            DigitizeEngine digitizeEngine2 = new DigitizeEngine();
            try
            {
                digitizeEngine = digitizeEngine2;

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
