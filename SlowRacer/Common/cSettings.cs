using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SlowRacer.Common
{
    public class cSettings
    {
        public System.Guid UidYou { get; set; }

        public string hostname  { get; set; }
        public int port { get; set; }

        public bool IsHost { get; set; }

        public string nickname { get; set; }

    }
}
