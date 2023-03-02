using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media;

namespace SlowRacer.Common
{
    internal class cPlayer
    {

        public Guid Uid { get; private set; }

        public Color color { get; set; }

        public cPlayer()
        {
            
            Uid = Guid.NewGuid();


        }
    }
}
