using System;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Controls;

namespace SlowRacer.Common
{
    internal class cCar
    {
        public enum TypeDir
        {
            cw, ccw
        }

        public TypeDir typeDir { get; set; }


        public enum TypeDriver
        {
            you, ai, opponent
        }

        public TypeDriver typeDriver { get; set; }=  TypeDriver.ai;

        public Guid Uid { get; private set; }


        public int Lap { get; set; } = 1;

        public bool LapCounted { get; set; } = true;  

        public double X { get; set; }
        public double Y { get; set; }

        public void SetDirection(int direction)
        {
            switch (direction)
            {
                case 0:
                    DirectionX = 0;
                    DirectionY = -1;
                    break;

                case 1:
                    DirectionX = 1;
                    DirectionY = -1;
                    break;

                case 2:
                    DirectionX = 1;
                    DirectionY = 0;
                    break;

                case 3:
                    DirectionX = 1;
                    DirectionY = 1;
                    break;

                case 4:
                    DirectionX = 0;
                    DirectionY = 1;
                    break;

                case 5:
                    DirectionX = -1;
                    DirectionY = 1;
                    break;

                case 6:
                    DirectionX = -1;
                    DirectionY = 0;
                    break;

                case 7:
                    DirectionX = -1;
                    DirectionY = -1;
                    break;

                default:
                    break;
            }
        }

        public bool OncomingTraffic { get; set; }

        public int Direction { get; set; }

        public double DirectionX { get; private set; }
        public double DirectionY { get; private set; }

        public int Speed { get; set; }

        public double NextStep { get; set; } = 0;

        public double Width { get; set; }
        public double Height { get; set; }
        public Color Color { get; set; }
        public UIElement UIElement { get; private set; }

        public cCar(Image image)
        {
            //Width = image.ActualWidth;
            // Height = image.ActualHeight;
            Width = image.Width;
            Height = image.Height;
            UIElement = image;
            Uid = Guid.NewGuid();

            
        }
    }
}