﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SlowRacer.Common
{
    internal class cTrack
    {

        public class cRGB
        {
            public byte red { get; set; }
            public byte green { get; set; }
            public byte blue { get; set; }

        }


        public int MaxSpeed { get; set; }
        public int MinSpeed { get; set; }

        public int Laps { get; set; }


        public byte[][]? RGBtrackArray { get; set; }


        //public WriteableBitmap? track { get; set; }
        public BitmapImage? background { get; set; }

        public int StartXccw { get; set; }
        public int StartYccw { get; set; }

        public int StartXcw { get; set; }
        public int StartYcw { get; set; }

        public int StartDirectioncw { get; set; }
        public int StartDirectionccw { get; set; }

        public int AICarscw { get; set; }
        public int AICarsccw { get; set; }


        public cRGB GetRGB(int X, int Y)
        {
            cRGB RGB = new cRGB();
            RGB.red = RGBtrackArray[Y][X * 3];
            RGB.green = RGBtrackArray[Y][X * 3 + 1];
            RGB.blue = RGBtrackArray[Y][X * 3 + 2];            

            return RGB; 
        }

    }
}
