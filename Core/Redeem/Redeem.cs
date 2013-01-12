using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dojo.Core.Redeem
{
    class Redeem
    {
        /* Dojo | Club Penguin Emulator
         * Written by nich (http://pure.so/)
         * Thanks to Cygnui's cyCP for help on a couple things. :)
         */

        public List<RedeemReward> RedeemItems = new List<RedeemReward>();
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
                    string reward = elemList[i].Attributes["prize"].Value;
                    RedeemItems.Add(new RedeemReward(code, type, Convert.ToInt32(reward)));
                }
            }
            catch
            {
                Logger.WriteError("redeem.xml not found, or contains invalid data. Please check the config directory.");
            }
        }
    }
}
