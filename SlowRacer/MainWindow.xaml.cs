using SlowRacer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace SlowRacer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random random = new Random(DateTime.Now.Millisecond);
        private List<cCar> cars = new List<cCar>();
        private cTrack ActivTrack = new cTrack();
        private DateTime lastfpsTime;
        private DateTime lastRenderTime = DateTime.Now;
        private int frameCount;
       

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(HandyTools.AppSavePath)) CreateAppSavePathWithDefaults();
            ActivTrack = HandyTools.LoadTrack(HandyTools.AppSavePath + "Tracks\\DefaultTrack");

            TrackImage.Source = ActivTrack.background;
            TrackImage.Width = ActivTrack.background.PixelWidth;
            TrackImage.Height = ActivTrack.background.PixelHeight;
            
            for (int i = 0; i < ActivTrack.AICarsccw; i++)
            {
                // var car = new cCar();
                var carImage = new BitmapImage(new Uri(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\car.png", UriKind.Absolute));
                var image = new Image();
                image.Source = carImage;

                cCar car = new cCar(image);
                car.SetDirection(ActivTrack.StartDirectionccw);
                car.X = ActivTrack.StartXccw;
                car.Y = ActivTrack.StartYccw;
                car.Direction = ActivTrack.StartDirectionccw;

                car.Width = carImage.Width;
                car.Height = carImage.Height;

                car.Speed = random.Next(10, 60);

                cars.Add(car);
                // canvas.Children.Add(car.UIElement);
            }

            for (int i = 0; i < ActivTrack.AICarscw; i++)
            {
                // var car = new cCar();
                var carImage = new BitmapImage(new Uri(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\car.png", UriKind.Absolute));
                var image = new Image();
                image.Source = carImage;

                cCar car = new cCar(image);
                car.SetDirection(ActivTrack.StartDirectioncw);
                car.X = ActivTrack.StartXcw;
                car.Y = ActivTrack.StartYcw;
                car.Direction = ActivTrack.StartDirectioncw;

                car.Width = carImage.Width;
                car.Height = carImage.Height;

                car.Speed = random.Next(10, 60);

                cars.Add(car);
                // canvas.Children.Add(car.UIElement);
            }

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void CreateAppSavePathWithDefaults()
        {
            Directory.CreateDirectory(HandyTools.AppSavePath);
            Directory.CreateDirectory(HandyTools.AppSavePath + "Tracks");
            Directory.CreateDirectory(HandyTools.AppSavePath + "Logs");
            Directory.CreateDirectory(HandyTools.AppSavePath + "Tracks\\DefaultTrack");

            HandyTools.SaveResourceToFile("Images/DefaultTrack/track1600x640.pdn", HandyTools.AppSavePath + "Tracks\\DefaultTrack\\track1600x640.pdn");
            HandyTools.SaveResourceToFile("Images/DefaultTrack/background.png", HandyTools.AppSavePath + "Tracks\\DefaultTrack\\background.png");
            HandyTools.SaveResourceToFile("Images/DefaultTrack/track.png", HandyTools.AppSavePath + "Tracks\\DefaultTrack\\track.png");
            HandyTools.SaveResourceToFile("Images/DefaultTrack/car.png", HandyTools.AppSavePath + "Tracks\\DefaultTrack\\car.png");

            HandyTools.Writeini(HandyTools.AppSavePath + "Settings.ini", "nickname", "Main", "nobody");

            HandyTools.Writeini(HandyTools.AppSavePath + "Settings.ini", "hostname", "Server", "gaming.easyfactuur.com");
            HandyTools.Writeini(HandyTools.AppSavePath + "Settings.ini", "port", "Server", "8090");

            HandyTools.Writeini(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\TrackSettings.ini", "StartXcw", "StartFinish", "1424");
            HandyTools.Writeini(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\TrackSettings.ini", "StartYcw", "StartFinish", "431");

            HandyTools.Writeini(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\TrackSettings.ini", "StartXccw", "StartFinish", "1459");
            HandyTools.Writeini(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\TrackSettings.ini", "StartYccw", "StartFinish", "447");

            HandyTools.Writeini(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\TrackSettings.ini", "StartDirectionccw", "StartFinish", "1");
            HandyTools.Writeini(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\TrackSettings.ini", "StartDirectioncw", "StartFinish", "5");

            HandyTools.Writeini(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\TrackSettings.ini", "AICarsccw", "StartFinish", "15");
            HandyTools.Writeini(HandyTools.AppSavePath + "Tracks\\DefaultTrack\\TrackSettings.ini", "AICarscw", "StartFinish", "10");
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {         

            TimeSpan elapsed = DateTime.Now - lastRenderTime;

            if (elapsed.Milliseconds >= -10)
            {
                lastRenderTime = DateTime.Now;

                foreach (var car in cars)
                {
                    car.NextStep += car.Speed * elapsed.TotalSeconds;

                    if ((int)car.NextStep < 1) continue;

                    int orgDirection = car.Direction;
                    

                    int step = 1;
                    int loopcount = 0;

                    while (car.NextStep > 1)
                    {
                        loopcount = loopcount + 1;
                        var tryNewXY = ActivTrack.GetRGB((int)(car.X + car.DirectionX), (int)(car.Y + car.DirectionY));

                        tbXY.Text = "X" + ((int)(car.X + car.DirectionX)).ToString() + "  Y" + ((int)(car.Y + car.DirectionY)).ToString();
                        if (tryNewXY.red > 0 || tryNewXY.green > 0 || tryNewXY.blue > 0)
                        {
                            car.X = car.X + car.DirectionX;
                            car.Y = car.Y + car.DirectionY;
                            
                            car.NextStep = car.NextStep - .3;
                            step = 1;
                            loopcount = 0;

                            if (random.Next(0, 100) == 1)
                            {
                                
                                car.Speed = car.Speed+random.Next(-20, 20);
                                if (car.Speed < 20) car.Speed = 20;
                                if (car.Speed >150) car.Speed = 150;

                            }

                            orgDirection = car.Direction;
                            continue;
                        }

                        if (loopcount >= 3)
                        {
                            loopcount = 0;
                            step = -1;
                            car.Direction = orgDirection;
                        }

                        car.Direction = car.Direction + step;
                        if (car.Direction > 7) car.Direction = 0;
                        if (car.Direction < 0) car.Direction = 7;
                        car.SetDirection(car.Direction);
                    }

                    // List<Point> outlinePoints = GetCircleOutlinePoints((int)NewX, (int)NewY, 2);

                    if (!canvas.Children.Contains(car.UIElement))
                    {
                        canvas.Children.Add(car.UIElement);
                    }

                    Canvas.SetLeft(car.UIElement, (int)(car.X - (car.Width / 2)));
                    Canvas.SetTop(car.UIElement, (int)(car.Y - (car.Height / 2)));
                }

                lastRenderTime = DateTime.Now;
              
            }

            // Update the FPS counter and display it on the screen
            TimeSpan elapsed2 = DateTime.Now - lastfpsTime;
            frameCount++;
            if (elapsed2.TotalMilliseconds >= 1000)
            {
                fpsTextBlock.Text = string.Format("FPS: {0:0}", frameCount);
                frameCount = 0;
                lastfpsTime = DateTime.Now;
            }
        }
    }
}