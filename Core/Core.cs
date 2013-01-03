using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Dojo.Core
{
    class Core
    {
        /* Dojo | Club Penguin Emulator
         * Written by Seether (http://tiber.me/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public static string ConstructServers()
        {
            String part = Config.ServerHost + ":" + Config.GamePort + ":Powered by Dojo " + Server.version + ":0";
            return part;
        }
        public static void Setup()
        {
            Console.Title = "Dojo BETA " + Server.version;

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Dojo Club Penguin Emulator");
            Console.WriteLine("This version of Dojo should be used for testing purposes only.");
            Console.WriteLine("Although it is not required, it is surely recommended.\n");
            Console.WriteLine("Version: " + Server.version + " BETA");
            Console.WriteLine("License: " + Config.LicenseKey + "\n");
            Console.WriteLine("Please don't share your license, it will only result in trouble.\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
