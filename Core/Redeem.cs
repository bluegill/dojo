using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dojo.Core
{
    class Redeem
    {
        /* Dojo | Club Penguin Emulator
         * Written by manipulate (http://tiber.me/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public List<string> RedeemItems = new List<string>();
        public void loadXML()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("config/redeem.xml");
                XmlNodeList elemList = doc.GetElementsByTagName("reward");
                for (int i = 0; i < elemList.Count; i++)
                {
                    string code = elemList[i].Attributes["code"].Value;
                    string type = elemList[i].Attributes["type"].Value;
                    string reward = elemList[i].Attributes["reward"].Value;
                    RedeemItems.Add("code=" + code + "&type=" + type + "&reward=" + reward + ";");
                }
            }
            catch
            {
                Logger.WriteError("redeem.xml not found, or contains invalid data. Please check the config directory.");
            }
        }
    }
}
