using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dojo.Core
{
    class Config
    {
        /* Dojo | Club Penguin Emulator
         * Written by nich (http://pure.so/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public static XmlReader config;

        public static int LoginPort;
        public static int GamePort;

        public static String LicenseKey;

        public static String ServerHost;
        public static String ServerDebug;

        public static String MySqlHost;
        public static String MySqlUser;
        public static String MySqlPass;
        public static String MySqlDatabase;

        public static String BotName;

        public static void loadXML()
        {
            string currentNode = null;
            try
            {
                config = new XmlTextReader("config/dojo.xml");
                while (config.Read())
                {
                    switch (config.NodeType)
                    {
                        case XmlNodeType.Element:
                            currentNode = config.Name;
                            break;
                        case XmlNodeType.Text:
                            switch (currentNode)
                            {
                                case "login_port":
                                    LoginPort = int.Parse(config.Value);
                                    break;
                                case "game_port":
                                    GamePort = int.Parse(config.Value);
                                    break;
                                case "server_host":
                                    ServerHost = config.Value;
                                    break;
                                case "server_debug":
                                    ServerDebug = config.Value;
                                    break;
                                case "mysql_host":
                                    MySqlHost = config.Value;
                                    break;
                                case "mysql_user":
                                    MySqlUser = config.Value;
                                    break;
                                case "mysql_pass":
                                    MySqlPass = config.Value;
                                    break;
                                case "mysql_database":
                                    MySqlDatabase = config.Value;
                                    break;
                                case "bot_name":
                                    BotName = config.Value;
                                    break;
                                case "dojo_license":
                                    LicenseKey = config.Value;
                                    break;
                                default: break;
                            }
                            break;
                    }
                }
            }
            catch
            {
                Logger.WriteError("dojo.xml not found, or contains invalid data. Please check the config directory.");
            }
        }
    }
}
