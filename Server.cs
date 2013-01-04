using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Dojo.Core;

namespace Dojo
{
    class Server
    {
        /* Dojo | Club Penguin Emulator
         * Written by manipulate (http://tiber.me/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public static double version = 0.5;
        public static MySQL MySQL;
        public static Redeem Redeem;
        public static void Main(string[] args)
        {
            Config.loadXML();
            Redeem = new Redeem();
            Redeem.loadXML();
            Core.Core.Setup();
            if (Convert.ToDouble(Helper.GetFileContents("http://core.dojo.pure.so/version")) != version)
            {
                Logger.WriteNotice("New version available! Please visit http://dojo.pure.so/ to download!");
            }
            Licensing Licensing = new Licensing();
            MySQL = new MySQL();
            Login Login = new Login();
            Thread LoginThread = new System.Threading.Thread(Login.AcceptClient);
            LoginThread.Start();
            Login.Create();

            Game Game = new Game();
            Thread GameThread = new System.Threading.Thread(Game.AcceptClient);
            GameThread.Start();
            Game.Create(Login);

            while (true) { }
        }
    }
}