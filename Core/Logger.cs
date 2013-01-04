using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dojo.Core
{
    class Logger
    {
        /* Dojo | Club Penguin Emulator
         * Written by nich (http://pure.so/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public static void Write(String message)
        {
            Console.WriteLine("[GENERAL]> " + message);
        }
        public static void WriteError(String message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR]> " + message);
            while (true)
            {
                Console.Read();
            }
        }
        public static void WriteNotice(String message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("[NOTICE]> " + message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
