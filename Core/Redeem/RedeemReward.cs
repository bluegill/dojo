using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dojo.Core.Redeem {
    class RedeemReward {
        public string code;
        public string type;
        public int id;
        public RedeemReward(string code, string type, int id) {
            this.code = code;
            this.type = type;
            this.id = id;
        }
    }
}
