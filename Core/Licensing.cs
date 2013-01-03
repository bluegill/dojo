using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dojo.Core
{
    class Licensing
    {
        /* Dojo | Club Penguin Emulator
         * Written by manipulate (http://tiber.me/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public Licensing()
        {
            Logger.Write("Verifying your license... please wait...\r\n");
            String check = Helper.GetFileContents("http://core.dojo.pure.so/license/check.php?key=" + Config.LicenseKey);
            if (check == "KEY_VALID")
            {
                // continue
            }
            else if (check == "KEY_INVALID")
            {
                Logger.WriteError("Your license key is NOT valid. Halting all operations...");
            }
            else
            {
                Logger.WriteError("Your license key MUST be exactly 15 characters long. Halting all operations...");
            }
        }
    }
}
