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
            this.SizeToContent = SizeToContent.Manual;
            this.Width = this.Width + 60;
            this.Height = this.Height + 25;

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

                car.Width = carImage.Width;
                car.Height = carImage.Height;

                car.Speed = random.Next(10, 100);

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
                    var StepX = car.DirectionX * car.Speed * elapsed.TotalSeconds;
                    var StepY = car.DirectionY * car.Speed * elapsed.TotalSeconds;

                    var NewX = car.X + car.DirectionX * 60 * elapsed.TotalSeconds;
                    var NewY = car.Y + car.DirectionY * 60 * elapsed.TotalSeconds;
                    //ActivTrack.track
                    var RGB = ActivTrack.GetRGB((int)NewX, (int)NewY);



                    Canvas.SetLeft(car.UIElement, car.X);
                    Canvas.SetTop(car.UIElement, car.Y);
                }

                /*foreach (var car in cars)
                {
                    var StepX = car.DirectionX * car.Speed * elapsed.TotalSeconds;
                    var StepY = car.DirectionY * car.Speed * elapsed.TotalSeconds;

                    car.X += StepX;
                    car.Y += StepY;

                    // Check if the car has gone off the edge of the canvas
                    if (car.X > canvas.Width - car.Width || car.X < 0 || car.Y > canvas.Height - car.Height || car.Y < 0)
                    {
                        car.X = 800;
                        car.Y = 300;
                        car.SetDirection(random.Next(0, 8));
                    }

                    // Set the position of the car
                    Canvas.SetLeft(car.UIElement, car.X);
                    Canvas.SetTop(car.UIElement, car.Y);

                    // Add the car to the canvas if it's not already there
                    if (!canvas.Children.Contains(car.UIElement))
                    {
                        canvas.Children.Add(car.UIElement);
                    }
                }*/
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