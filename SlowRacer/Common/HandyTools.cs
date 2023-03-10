using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SlowRacer.Common
{
    internal class HandyTools
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);      

        public static string ExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + '\\';
        public static string ExeName = System.AppDomain.CurrentDomain.FriendlyName;
        public static string AppSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + '\\' + HandyTools.ExeName + '\\';

        public static void SaveResourceToFile(string resourceName, string filePath)
        {
            using (Stream stream = Application.GetResourceStream(new Uri(resourceName, UriKind.RelativeOrAbsolute)).Stream)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                File.WriteAllBytes(filePath, buffer);

                stream.Close();
            }
        }

        public static string[] GetSubdirectories(string path)
        {
            string[] directories = Directory.GetDirectories(path);

            for (int i = 0; i < directories.Count(); i++)
            {
                directories[i] = Path.GetFileName(directories[i]);
            }

            return directories;
        }

        public static WriteableBitmap ReplaceColor(WriteableBitmap bitmap, System.Windows.Media.Color oldColor, System.Windows.Media.Color newColor)
        {
            WriteableBitmap newBitmap = new WriteableBitmap(bitmap);

            byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];

            // Copy the pixel data from the original bitmap into the byte array
            bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);

            // Iterate through each pixel in the byte array
            for (int i = 0; i < pixels.Length; i += 4)
            {
                System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);

                // Replace the color of the current pixel if it matches the old color and is not transparent
                if (color == System.Windows.Media.Color.FromArgb(255, oldColor.R, oldColor.G, oldColor.B) && color.A != 0)
                {
                    pixels[i + 3] = newColor.A;
                    pixels[i + 2] = newColor.R;
                    pixels[i + 1] = newColor.G;
                    pixels[i] = newColor.B;
                }
            }
            // Copy the modified pixel data from the byte array into the new bitmap
            newBitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, bitmap.PixelWidth * 4, 0);

            return newBitmap;// ConvertWriteableBitmapToBitmapImage(newBitmap);
        }

        public static byte[][] bitmapImage2RGBtrackArray(BitmapImage bitmapImage)
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);

            // Copy pixel data to byte array
            byte[] pixelData = new byte[writeableBitmap.PixelWidth * writeableBitmap.PixelHeight * 4];
            writeableBitmap.CopyPixels(pixelData, writeableBitmap.PixelWidth * 4, 0);

            // Rearrange pixel data to get RGB color array
            byte[][] rgbColorArray = new byte[writeableBitmap.PixelHeight][];
            for (int i = 0; i < writeableBitmap.PixelHeight; i++)
            {
                rgbColorArray[i] = new byte[writeableBitmap.PixelWidth * 3];
                for (int j = 0; j < writeableBitmap.PixelWidth; j++)
                {
                    int offset = i * writeableBitmap.PixelWidth * 4 + j * 4;
                    rgbColorArray[i][j * 3] = pixelData[offset + 2];   // Red
                    rgbColorArray[i][j * 3 + 1] = pixelData[offset + 1]; // Green
                    rgbColorArray[i][j * 3 + 2] = pixelData[offset];     // Blue
                }
            }
            return rgbColorArray;
        }

        public static string Readini(string FileName, string Key, string Section, string Default = null)
        {
            var RetVal = new StringBuilder(255);

            GetPrivateProfileString(Section, Key, "", RetVal, 255, FileName);
            if (RetVal.Length < 1 && Default != null)
            {
                Writeini(FileName, Key, Section, Default);
                return Default;
            }
            return RetVal.ToString();
        }

        public static void Writeini(string FileName, string Key, string Section, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, FileName);
        }

        public static Boolean WriteToLog(string Message, string Subject, Boolean ResetLog = false)
        {
            if (Directory.Exists(HandyTools.AppSavePath + "Logs") == false) Directory.CreateDirectory(HandyTools.AppSavePath + "Logs");

            string Filename = HandyTools.AppSavePath + "Logs" + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            string newline = DateTime.Now.ToString("[HH:mm:ss]") + "[" + Subject + "] \"" + Message + "\"";
            try
            {
                if (!File.Exists(Filename) || ResetLog == true)
                {
                    File.WriteAllText(Filename, newline + Environment.NewLine);
                }
                else
                {
                    File.AppendAllText(Filename, newline + Environment.NewLine);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static cTrack LoadTrack(string Path, ref cPlayer[] Players)
        {
            cTrack track = new cTrack();

            track.background = new BitmapImage(new Uri(Path + "\\background.png", UriKind.Absolute));

            track.Width = track.background.PixelWidth;
            track.Height = track.background.PixelHeight;

            track.RGBtrackArray = bitmapImage2RGBtrackArray(new BitmapImage(new Uri(Path + "\\track.png", UriKind.Absolute)));

            track.AICarsccw = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "AICarsccw", "StartFinish", "12"));
            track.AICarscw = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "AICarscw", "StartFinish", "7"));

            track.Laps = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "DefaultLaps", "StartFinish", "5"));

            track.StartXccw = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "StartXccw", "StartFinish", "70"));
            track.StartXcw = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "StartXcw", "StartFinish", "70"));

            track.StartYccw = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "StartYccw", "StartFinish", "70"));
            track.StartYcw = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "StartYcw", "StartFinish", "70"));

            track.StartDirectionccw = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "StartDirectionccw", "StartFinish", "1"));
            track.StartDirectioncw = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "StartDirectioncw", "StartFinish", "5"));

            track.MaxSpeed = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "MaxSpeed", "Cars", "70"));
            track.MinSpeed = int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "MinSpeed", "Cars", "30"));

            cTrack.cRGB NewColor = new cTrack.cRGB();

            NewColor.red = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player1RGB_Red", "Cars", "200"));
            NewColor.green = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player1RGB_Green", "Cars", "0"));
            NewColor.blue = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player1RGB_Blue", "Cars", "0"));
            Players[0].color = Color.FromRgb(NewColor.red, NewColor.green, NewColor.blue);

            NewColor.red = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player2RGB_Red", "Cars", "0"));
            NewColor.green = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player2RGB_Green", "Cars", "200"));
            NewColor.blue = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player2RGB_Bluen", "Cars", "0"));
            Players[1].color = Color.FromRgb(NewColor.red, NewColor.green, NewColor.blue);

            NewColor.red = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player3RGB_Red", "Cars", "0"));
            NewColor.green = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player3RGB_Green", "Cars", "0"));
            NewColor.blue = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player3RGB_Blue", "Cars", "200"));
            Players[2].color = Color.FromRgb(NewColor.red, NewColor.green, NewColor.blue);

            NewColor.red = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player4RGB_Red", "Cars", "0"));
            NewColor.green = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player4RGB_Green", "Cars", "170"));
            NewColor.blue = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player4RGB_Blue", "Cars", "170"));
            Players[3].color = Color.FromRgb(NewColor.red, NewColor.green, NewColor.blue);

            NewColor.red = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player5RGB_Red", "Cars", "150"));
            NewColor.green = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player5RGB_Green", "Cars", "50"));
            NewColor.blue = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "Player5RGB_Blue", "Cars", "100"));
            Players[4].color = Color.FromRgb(NewColor.red, NewColor.green, NewColor.blue);

            NewColor.red = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "AICarRGBcw_Red", "Cars", "10"));
            NewColor.green = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "AICarRGBcw_Green", "Cars", "0"));
            NewColor.blue = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "AICarRGBcw_Blue", "Cars", "10"));
            track.CarRGBcw = Color.FromRgb(NewColor.red, NewColor.green, NewColor.blue);

            NewColor.red = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "AICarRGBccw_Red", "Cars", "200"));
            NewColor.green = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "AICarRGBccw_Green", "Cars", "200"));
            NewColor.blue = (byte)int.Parse(HandyTools.Readini(Path + "\\TrackSettings.ini", "AICarRGBccw_Blue", "Cars", "0"));
            track.CarRGBccw = Color.FromRgb(NewColor.red, NewColor.green, NewColor.blue);

            return track;
        }

        internal static cCar IsInCollitionWith(cCar car, Dictionary<Guid, cCar> cars)
        {
            foreach (var carItem in cars.Values)
            {
                double distance = 0;

                distance = Math.Abs(carItem.X - car.X) + Math.Abs(carItem.Y - car.Y);
                if (distance <= car.Width + 4 && car.Uid != carItem.Uid && car.typeDir == carItem.typeDir)

                {
                    if ((car.Direction == 6 || car.Direction == 5 || car.Direction == 7) && car.X > carItem.X) return carItem;
                    if ((car.Direction == 2 || car.Direction == 1 || car.Direction == 3) && car.X < carItem.X) return carItem;

                    if ((car.Direction == 0 || car.Direction == 7 || car.Direction == 1) && car.Y > carItem.Y) return carItem;
                    if ((car.Direction == 4 || car.Direction == 5 || car.Direction == 3) && car.Y < carItem.Y) return carItem;
                }
            }
            return null;
        }

        internal static cCar SwitchLanes(cCar carValues, cTrack activeTrack, Dictionary<Guid, cCar> cars)
        {
            // check if current position allow lanesswitsh, if line is having color red in it then not
            if (activeTrack.GetRGB((int)carValues.X, (int)carValues.Y).red > 50) return carValues;

            cCar dummyCar = new cCar(null);

            double PosX = carValues.X;
            double PosY = carValues.Y;

            //calculate what direction to look for other lanes
            switch (carValues.Direction)
            {
                case 0:
                case 4:
                    dummyCar.SetDirection(6);
                    break;

                case 1:
                case 5:
                    dummyCar.SetDirection(7);
                    break;

                case 2:
                case 6:
                    dummyCar.SetDirection(0);
                    break;

                case 3:
                case 7:
                    dummyCar.SetDirection(5);
                    break;
            }

            int X1 = (int)(carValues.X + dummyCar.DirectionX);
            int Y1 = (int)(carValues.Y + dummyCar.DirectionY);
            int X2 = (int)(carValues.X - dummyCar.DirectionX);
            int Y2 = (int)(carValues.Y - dummyCar.DirectionY);

            bool FoundOtherLanes = false;

            cTrack.cRGB GRB = activeTrack.GetRGB((int)carValues.X, (int)carValues.Y);

            
            bool[] RedlineDetected = new bool[10];

            for (int i = 0; i < 60; i++)
            {
                X1 += (int)dummyCar.DirectionX;
                X2 -= (int)dummyCar.DirectionX;
                Y1 += (int)dummyCar.DirectionY;
                Y2 -= (int)dummyCar.DirectionY;

                if (X1 < 10) X1 = 10;
                if (X2 < 10) X2 = 10;
                if (Y1 < 10) Y1 = 10;
                if (Y2 < 10) Y2 = 10;

                if (X1 + 10 > activeTrack.Width) X1 = activeTrack.Width - 10;
                if (X2 + 10 > activeTrack.Width) X2 = activeTrack.Width - 10;
                if (Y1 + 10 > activeTrack.Height) Y1 = activeTrack.Height - 10;
                if (Y2 + 10 > activeTrack.Height) Y2 = activeTrack.Height - 10;

                GRB = activeTrack.GetRGB(X1, Y1);
                if (GRB.red > 50) RedlineDetected[0]=true;
                if (RedlineDetected[0] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X1; PosY = Y1;
                    FoundOtherLanes = true;
                    break;
                }

                GRB = activeTrack.GetRGB(X1 + 1, Y1);
                if (GRB.red > 50) RedlineDetected[1] = true;
                if (RedlineDetected[1] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X1 + 1; PosY = Y1;
                    FoundOtherLanes = true;
                    break;
                }

                GRB = activeTrack.GetRGB(X1 - 1, Y1);
                if (GRB.red > 50) RedlineDetected[2] = true;
                if (RedlineDetected[2] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X1 - 1; PosY = Y1;
                    FoundOtherLanes = true;
                    break;
                }

                GRB = activeTrack.GetRGB(X1, Y1 + 1);
                if (GRB.red > 50) RedlineDetected[3] = true;
                if (RedlineDetected[3] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X1; PosY = Y1 + 1;
                    FoundOtherLanes = true;
                    break;
                }

                GRB = activeTrack.GetRGB(X1, Y1 - 1);
                if (GRB.red > 50) RedlineDetected[4] = true;
                if (RedlineDetected[4] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X1; PosY = Y1 - 1;
                    FoundOtherLanes = true;
                    break;
                }

                // **********************************************************************************************

                GRB = activeTrack.GetRGB(X2, Y2);
                if (GRB.red > 50) RedlineDetected[5] = true;
                if (RedlineDetected[5] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X2; PosY = Y2;
                    FoundOtherLanes = true;
                    break;
                }

                GRB = activeTrack.GetRGB(X2 + 1, Y2);
                if (GRB.red > 50) RedlineDetected[6] = true;
                if (RedlineDetected[6] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X2 + 1; PosY = Y2;
                    FoundOtherLanes = true;
                    break;
                }

                GRB = activeTrack.GetRGB(X2 - 1, Y2);
                if (GRB.red > 50) RedlineDetected[7] = true;
                if (RedlineDetected[7] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X2 - 1; PosY = Y2;
                    FoundOtherLanes = true;
                    break;
                }

                GRB = activeTrack.GetRGB(X2, Y2 + 1);
                if (GRB.red > 50) RedlineDetected[8] = true;
                if (RedlineDetected[8] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X2 - 1; PosY = Y2 + 1;
                    FoundOtherLanes = true;
                    break;
                }

                GRB = activeTrack.GetRGB(X2, Y2 - 1);
                if (GRB.red > 50) RedlineDetected[9] = true;
                if (RedlineDetected[9] == false && (GRB.green > 50 || GRB.blue > 50))
                {
                    PosX = X2 - 1; PosY = Y2 - 1;
                    FoundOtherLanes = true;
                    break;
                }
            }
            if (FoundOtherLanes)
            {
                double OrgPosX = carValues.X; double OrgPosY = carValues.Y;
                carValues.typeDir = (carValues.typeDir==cCar.TypeDir.ccw) ? cCar.TypeDir.cw : cCar.TypeDir.ccw;

                carValues.X = PosX; carValues.Y = PosY;
                if (IsInCollitionWith(carValues, cars) != null)
                {
                    carValues.typeDir = (carValues.typeDir == cCar.TypeDir.ccw) ? cCar.TypeDir.cw : cCar.TypeDir.ccw;
                    carValues.X = OrgPosX; carValues.Y = OrgPosY;
                }
                else
                {
                    carValues.typeDir = (GRB.blue > 50) ? cCar.TypeDir.cw : cCar.TypeDir.ccw;
                    carValues.DrivingOnWrongLanes = (GRB.blue > 50) ? true : false;
                }
            }
            return carValues;
        }
    }
}