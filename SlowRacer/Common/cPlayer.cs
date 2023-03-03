using System;
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