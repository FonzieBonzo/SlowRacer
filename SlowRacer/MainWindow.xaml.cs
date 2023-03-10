using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SlowRacer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WebSocketSharp;
using static SlowRacer.Common.cCar;
using Image = System.Windows.Controls.Image;

namespace SlowRacer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random random = new Random(DateTime.Now.Millisecond);
        private Dictionary<System.Guid, cCar> cars = new Dictionary<System.Guid, cCar>();
        private cPlayer[] Players = new cPlayer[5];
        private cTrack ActiveTrack = new cTrack();
        private DateTime lastfpsTime;
        private DateTime lastRenderTime = DateTime.Now;
        private int frameCount;

        private string OldKeysForGameHost = "";

        private DateTime dtCountDownStart;

        private DateTime dtKeyE,dtKeyW, dtKeyS, dtKeySpace, dtKeyRightShift, dtKeyUp, dtKeyDown;

        public cSettings Settings = new cSettings();
        private WebSocket WS;

        private WaveOut sndHorn1 = new WaveOut();
        private WaveOut sndHorn2 = new WaveOut();
        private WaveOut sndCollitionBack = new WaveOut();
        private WaveOut sndCollitionfrontal = new WaveOut();
        private WaveOut sndStartRace = new WaveOut();
        private WaveOut sndFinishRace = new WaveOut();



        public MainWindow()
        {
            InitializeComponent();

            InitSounds();

            

            for (int i = 0; i < 5; i++)
            {
                Players[i] = new cPlayer();
            }

            if (!Directory.Exists(HandyTools.AppSavePath)) CreateAppSavePathWithDefaults();

            CheckRecources2File("Small Track v1.0");
            CheckRecources2File("ZigZag v1.0");
            CheckRecources2File("I Dare You v1.0");
            CheckRecources2File("Pitstop v1.0");
            CheckRecources2File("Island v1.0");
            CheckRecources2File("Highway v1.0");

            cbTracks.ItemsSource = HandyTools.GetSubdirectories(HandyTools.AppSavePath + "Tracks");

            Settings.hostname = HandyTools.Readini(HandyTools.AppSavePath + "Settings.ini", "hostname", "Server", "gaming.easyfactuur.com");
            Settings.port = int.Parse(HandyTools.Readini(HandyTools.AppSavePath + "Settings.ini", "port", "Server", "8090"));
            Settings.IsHost = (HandyTools.Readini(HandyTools.AppSavePath + "Settings.ini", "IsHost", "Server", "0") == "1") ? true : false;
            Settings.OnlineMode = (HandyTools.Readini(HandyTools.AppSavePath + "Settings.ini", "OnlineMode", "Main", "0") == "0") ? true : false;


            Settings.nickname = HandyTools.Readini(HandyTools.AppSavePath + "Settings.ini", "nickname", "Main", "nobody");

            if (Settings.OnlineMode)
            {

                WS = new WebSocket(@"ws://" + Settings.hostname + ":" + Settings.port.ToString() + "/benzie");
                WS.OnMessage += WS_OnMessage;
                WS.Connect();
                WSUpdate();
            }
            // WebSocket.ConnectAsync()

            lastRenderTime = DateTime.Now;
            cbTracks.SelectedItem = HandyTools.Readini(HandyTools.AppSavePath + "Settings.ini", "ActiveTrack", "main", "none");
        }

        private void InitSounds()
        {            
            Stream resourceStream;
            WaveFileReader reader;
            resourceStream = Application.GetResourceStream(new Uri("SoundFX/StartRace.wav", UriKind.RelativeOrAbsolute)).Stream;
            reader = new WaveFileReader(resourceStream);
            sndStartRace.Init(reader);   



            resourceStream = Application.GetResourceStream(new Uri("SoundFX/Carn Horn 1.wav", UriKind.RelativeOrAbsolute)).Stream;
            reader = new WaveFileReader(resourceStream);
            sndHorn1.Init(reader);

            resourceStream = Application.GetResourceStream(new Uri("SoundFX/Carn Horn 2.wav", UriKind.RelativeOrAbsolute)).Stream;
            reader = new WaveFileReader(resourceStream);
            sndHorn2.Init(reader);

            resourceStream = Application.GetResourceStream(new Uri("SoundFX/DM-CGS-18.wav", UriKind.RelativeOrAbsolute)).Stream;
            reader = new WaveFileReader(resourceStream);
            sndFinishRace.Init(reader);

            resourceStream = Application.GetResourceStream(new Uri("SoundFX/DM-CGS-48.wav", UriKind.RelativeOrAbsolute)).Stream;
            reader = new WaveFileReader(resourceStream);
            sndCollitionfrontal.Init(reader);

            resourceStream = Application.GetResourceStream(new Uri("SoundFX/DM-CGS-02.wav", UriKind.RelativeOrAbsolute)).Stream;
            reader = new WaveFileReader(resourceStream);
            sndCollitionBack.Init(reader);
        }

        private void CheckRecources2File(string TheName)
        {
            if (Directory.Exists(HandyTools.AppSavePath + "Tracks\\" + TheName)) return;
            Directory.CreateDirectory(HandyTools.AppSavePath + "Tracks\\" + TheName);
            HandyTools.SaveResourceToFile("Tracks/" + TheName + "/PaintNET.pdn", HandyTools.AppSavePath + "Tracks\\" + TheName + "\\PaintNET.pdn");
            HandyTools.SaveResourceToFile("Tracks/" + TheName + "/background.png", HandyTools.AppSavePath + "Tracks\\" + TheName + "\\background.png");
            HandyTools.SaveResourceToFile("Tracks/" + TheName + "/track.png", HandyTools.AppSavePath + "Tracks\\" + TheName + "\\track.png");
            HandyTools.SaveResourceToFile("Tracks/" + TheName + "/car.png", HandyTools.AppSavePath + "Tracks\\" + TheName + "\\car.png");
            HandyTools.SaveResourceToFile("Tracks/" + TheName + "/TrackSettings.ini", HandyTools.AppSavePath + "Tracks\\" + TheName + "\\TrackSettings.ini");
        }

        private void cbTracs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTracks.SelectedIndex == -1) return;
            prepaireRace(cbTracks.SelectedItem.ToString());
        }

        private void prepaireRace(String TheTrack)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            cars.Clear();
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = canvas.Children[i];
                if ((child is Image))
                {
                    string childName = (child as FrameworkElement)?.Name;
                    if (childName != "TrackImage") canvas.Children.Remove(child);
                }
            }

            //
            Settings.GameStatus = 1;
            ActiveTrack = HandyTools.LoadTrack(HandyTools.AppSavePath + "Tracks\\" + TheTrack, ref Players);

            canvas.Width = ActiveTrack.Width;
            canvas.Height = ActiveTrack.Height;

            TrackImage.Source = ActiveTrack.background;
            TrackImage.Width = ActiveTrack.background.PixelWidth;
            TrackImage.Height = ActiveTrack.background.PixelHeight;

            //generate counterclockwise AI cars
            for (int i = 0; i < ActiveTrack.AICarsccw; i++)
            {
                var carImage = new BitmapImage(new Uri(HandyTools.AppSavePath + "Tracks\\" + TheTrack + "\\car.png", UriKind.Absolute));
                WriteableBitmap newBitmap = new WriteableBitmap(carImage);
                WriteableBitmap newCarImage;
                if (i < 2)
                {
                    newCarImage = HandyTools.ReplaceColor(newBitmap, Color.FromRgb(0, 0, 0), Players[i].color);
                }
                else
                {
                    newCarImage = HandyTools.ReplaceColor(newBitmap, Color.FromRgb(0, 0, 0), ActiveTrack.CarRGBccw);
                }
                var image = new Image();
                image.Source = newCarImage;
                cCar car = new cCar(image);
                car.Speed = random.Next(ActiveTrack.MinSpeed, ActiveTrack.MaxSpeed + 1);
                if (i < 2)
                {
                    car.Speed = ActiveTrack.MinSpeed;

                    switch (i)
                    {
                        case 0:
                            car.typeDriver = TypeDriver.p1;
                            Settings.UidYou = car.Uid;
                            break;

                        case 1:
                            car.typeDriver = TypeDriver.p2;
                            break;

                        case 2:
                            car.typeDriver = TypeDriver.p3;
                            break;

                        case 3:
                            car.typeDriver = TypeDriver.p4;
                            break;

                        case 4:
                            car.typeDriver = TypeDriver.p5;
                            break;
                    }
                }
                else
                {
                    car.typeDriver = TypeDriver.ai;
                }
                car.typeDir = TypeDir.ccw;
                car.SetDirection(ActiveTrack.StartDirectionccw);
                car.X = ActiveTrack.StartXccw;
                car.Y = ActiveTrack.StartYccw;
                car.Direction = ActiveTrack.StartDirectionccw;
                car.Width = carImage.Width;
                car.Height = carImage.Height;

                cars.Add(car.Uid, car);
                canvas.Children.Add(car.UIElement);
            }

            //generate clockwise AI cars
            for (int i = 0; i < ActiveTrack.AICarscw; i++)
            {
                // var car = new cCar();
                var carImage = new BitmapImage(new Uri(HandyTools.AppSavePath + "Tracks\\" + TheTrack + "\\car.png", UriKind.Absolute));
                WriteableBitmap newBitmap = new WriteableBitmap(carImage);
                WriteableBitmap newCarImage = HandyTools.ReplaceColor(newBitmap, Color.FromRgb(0, 0, 0), ActiveTrack.CarRGBcw);
                //carImage =
                var image = new Image();
                image.Source = newCarImage;
                cCar car = new cCar(image);
                car.Lap = -1000;
                car.typeDir = TypeDir.cw;
                car.SetDirection(ActiveTrack.StartDirectioncw);
                car.X = ActiveTrack.StartXcw;
                car.Y = ActiveTrack.StartYcw;
                car.Direction = ActiveTrack.StartDirectioncw;
                car.Width = carImage.Width;
                car.Height = carImage.Height;
                car.Speed = random.Next(ActiveTrack.MinSpeed, ActiveTrack.MaxSpeed + 1);
                cars.Add(car.Uid, car);
                canvas.Children.Add(car.UIElement);
            }

            // set all ai cars straggling on the track
            for (int i = 0; i < 1000; i++)
            {
                foreach (var car in cars.Values)
                {
                    if (car.typeDriver != TypeDriver.ai) continue;
                    var NewCar = CalculateNextStep(car, 300, cars);
                    if (NewCar.typeDir == TypeDir.ccw) NewCar.Lap = 0;

                    cars[NewCar.Uid] = NewCar;
                }
            }

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void WS_OnMessage(object? sender, MessageEventArgs e)
        {

            //  tbxx.            .Text= e.Data.ToString();

            //HandyTools.WriteToLog(e.Data.ToString(), "key");       

            /*string[] data = e.Data.Split('|');
            if (data.Length == 8 && data[0] != _name && data[1] == _scene)
            {
                Partner partner = _partners.Find(x => x.Name == data[0]);
                if (partner == null)
                {
                    partner = new Partner(data[0]);
                    _partners.Add(partner);
                }

                partner.Data = data;
            }*/
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.GameStatus >= 5) return;

            sndStartRace.Volume = 0.08f;
            sndStartRace.Play();

            Settings.GameStatus = 2;
            tbBTN.Text = "Start 3";
            dtCountDownStart = DateTime.Now;
        }        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HandyTools.Writeini(HandyTools.AppSavePath + "Settings.ini", "ActiveTrack", "main", cbTracks.SelectedItem.ToString());
        }

        private void WSUpdate()
        {
            WS.Send("TEST");
        }

        

        private void SendKeyToGameHost(string newKeysForGameHost)
        {
            if (Settings.OnlineMode==false) return;
            WS.Send(newKeysForGameHost);
        }

        private void CreateAppSavePathWithDefaults()
        {
            Directory.CreateDirectory(HandyTools.AppSavePath);
            Directory.CreateDirectory(HandyTools.AppSavePath + "Tracks");            
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            double elapsed = (DateTime.Now - lastRenderTime).TotalMilliseconds;
            lastRenderTime = DateTime.Now;

            CheckKeys();

            foreach (var car in cars.Values)
            {
                cCar NewCar = car;

                if (Settings.GameStatus >= 5) NewCar = CalculateNextStep(car, elapsed, cars);

                cars[NewCar.Uid] = NewCar;

                if (NewCar.typeDriver == TypeDriver.p1)
                {
                    TB.Text = "lap:" + car.Lap.ToString() + "/" + ActiveTrack.Laps.ToString();
                }

                Canvas.SetLeft(NewCar.UIElement, (int)(NewCar.X - (NewCar.Width / 2)));
                Canvas.SetTop(NewCar.UIElement, (int)(NewCar.Y - (NewCar.Height / 2)));
            }

            if (Settings.GameStatus > 1 && Settings.GameStatus < 5 && DateTime.Now.Ticks - dtCountDownStart.Ticks > 1000 * 10000)
            {
                Settings.GameStatus += 1;
                tbBTN.Text = "Start " + (5 - Settings.GameStatus).ToString();
                dtCountDownStart = DateTime.Now;
            }

            lastRenderTime = DateTime.Now;

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

        private cCar CalculateNextStep(cCar car, double elapsed, Dictionary<Guid, cCar> cars)
        {
            if (DateTime.Now.Ticks - car.dtPenalty.Ticks < 1200 * 10000) return car;

            if (car.Lap <= ActiveTrack.Laps) { car.NextStep += car.Speed * (elapsed / 1000); }
            else
            {
                if (car.Speed > 0) car.Speed = car.Speed - 1;
                car.NextStep += car.Speed * (elapsed / 1000);
            }

            if ((int)car.NextStep < 1) return car;

            int orgDirection = car.Direction;

            int step = 1;
            int loopcount = 0;

            while (car.NextStep > 1)
            {
                loopcount = loopcount + 1;
                var tryNewXY = ActiveTrack.GetRGB((int)(car.X + car.DirectionX), (int)(car.Y + car.DirectionY));

                if ( tryNewXY.green > 50 || tryNewXY.blue > 50)
                {
                    cCar InCollCar = HandyTools.IsInCollitionWith(car, cars);

                    if (InCollCar != null)
                    {
                        if (car.Speed > InCollCar.Speed) car.Speed = InCollCar.Speed - 10;
                        if (car.Speed < ActiveTrack.MinSpeed) car.Speed = ActiveTrack.MinSpeed;
                        if (car.DrivingOnWrongLanes) { car.dtPenalty = DateTime.Now; }
                    }

                    int OldX = (int)car.X;
                    int OldY = (int)car.Y;

                    car.X = car.X + car.DirectionX;
                    car.Y = car.Y + car.DirectionY;

                    if (OldX != (int)car.X || OldY != (int)car.Y)
                    {
                        if (DateTime.Now.Ticks - car.dtPlusLap.Ticks > 3000 * 1000 && car.LapCounted == false && ((Math.Abs(car.X - ActiveTrack.StartXccw) <= 0 && Math.Abs(car.X - ActiveTrack.StartXccw) <= 0 && Math.Abs(car.Y - ActiveTrack.StartYccw) <= 1) || (Math.Abs(car.X - ActiveTrack.StartXcw) <= 1 && Math.Abs(car.Y - ActiveTrack.StartYcw) <= 1)))
                        {
                            car.Lap = car.Lap + 1;
                            car.LapCounted = true;
                            car.dtPlusLap = DateTime.Now;
                        }
                        else
                        {
                            car.LapCounted = false;
                        }
                    }

                    car.NextStep = car.NextStep - .3;
                    step = 1;
                    loopcount = 0;

                    if (car.typeDriver == TypeDriver.ai && random.Next(0, 50) == 1)
                    {
                        car.Speed = car.Speed + random.Next(-20, 20);
                        if (car.Speed < ActiveTrack.MinSpeed) car.Speed = ActiveTrack.MinSpeed;
                        if (car.Speed > ActiveTrack.MaxSpeed) car.Speed = ActiveTrack.MaxSpeed;
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
            return car;
        }

        private void CheckKeys()
        {
            cCar CarValues = new cCar(null);

            string NewKeysForGameHost = "";

            string keys = "";

            Guid Uidpx = CarValues.Uid;

            foreach (KeyValuePair<System.Guid, cCar> item in cars)
            {
                if (item.Value.typeDriver == TypeDriver.p1)
                {
                    CarValues = item.Value;
                    Uidpx = CarValues.Uid;
                    break;
                }
            }

            if (CarValues.typeDriver == TypeDriver.p1)
            {
                if (Keyboard.IsKeyDown(Key.E))
                {
                    NewKeysForGameHost += "E";
                    keys = keys + " E";

                    if (DateTime.Now.Ticks - dtKeyE.Ticks > 4000 * 10000)
                    {
                        dtKeyE = DateTime.Now;
                       
                        sndHorn1.Play();
                        
                    }
                }
                else { dtKeyE = DateTime.Now.AddMinutes(-1); }



                if (Keyboard.IsKeyDown(Key.W) && CarValues.Lap <= ActiveTrack.Laps)
                {
                    NewKeysForGameHost += "W";
                    keys = keys + " W";
                    if (DateTime.Now.Ticks - dtKeyW.Ticks > 400*10000)
                    {
                        dtKeyW = DateTime.Now;
                        if (HandyTools.IsInCollitionWith(CarValues, cars) == null && CarValues.Speed <= (ActiveTrack.MaxSpeed - 10)) CarValues.Speed += 10;
                    }
                }
                else { dtKeyW = DateTime.Now.AddMinutes(-1); }

                if (Keyboard.IsKeyDown(Key.S))
                {
                    NewKeysForGameHost += "S";
                    keys = keys + " S";
                    if (DateTime.Now.Ticks - dtKeyS.Ticks > 400*10000)
                    {
                        dtKeyS = DateTime.Now;
                        if (CarValues.Speed >= (ActiveTrack.MinSpeed + 10)) CarValues.Speed -= 10;
                    }
                }
                else { dtKeyS = DateTime.Now.AddMinutes(-1); }

                if (Keyboard.IsKeyDown(Key.Space))
                {
                    NewKeysForGameHost += "<>";
                    keys = keys + " Space";
                    if (DateTime.Now.Ticks - dtKeySpace.Ticks > 800 * 10000)
                    {
                        dtKeySpace = DateTime.Now;
                        CarValues = HandyTools.SwitchLanes(CarValues, ActiveTrack, cars);
                    }
                }
                else { dtKeySpace = DateTime.Now.AddMinutes(-1); }
            }

            if (CarValues.Uid == Settings.UidYou) TB2.Text = keys + " speed:" + CarValues.Speed.ToString();

            foreach (KeyValuePair<System.Guid, cCar> item in cars)
            {
                if (item.Value.typeDriver == TypeDriver.p2)
                {
                    CarValues = item.Value;
                    break;
                }
            }

            if (CarValues.typeDriver == TypeDriver.p2)
            {
                if (Keyboard.IsKeyDown(Key.NumPad8) && CarValues.Lap <= ActiveTrack.Laps)
                {
                    keys = keys + " Pad8";
                    if (DateTime.Now.Ticks - dtKeyUp.Ticks > 400*10000)
                    {
                        dtKeyUp = DateTime.Now;
                        if (HandyTools.IsInCollitionWith(CarValues, cars) == null && CarValues.Speed <= (ActiveTrack.MaxSpeed - 10)) CarValues.Speed += 10;
                    }
                }
                else { dtKeyUp = DateTime.Now.AddMinutes(-1); }

                if (Keyboard.IsKeyDown(Key.NumPad5))
                {
                    keys = keys + " Pad5";
                    if (DateTime.Now.Ticks - dtKeyDown.Ticks > 400 * 10000)
                    {
                        dtKeyDown = DateTime.Now;
                        if (CarValues.Speed >= (ActiveTrack.MinSpeed + 10)) CarValues.Speed -= 10;
                    }
                }
                else { dtKeyDown = DateTime.Now.AddMinutes(-1); }

                if (Keyboard.IsKeyDown(Key.Enter))
                {
                    keys = keys + " Enter";
                    if (DateTime.Now.Ticks - dtKeyRightShift.Ticks > 800 * 10000)
                    {
                        dtKeyRightShift = DateTime.Now;
                        CarValues = HandyTools.SwitchLanes(CarValues, ActiveTrack, cars);
                    }
                }
                else { dtKeyRightShift = DateTime.Now.AddMinutes(-1); }

                if (NewKeysForGameHost!= OldKeysForGameHost)
                {
                    SendKeyToGameHost(NewKeysForGameHost);
                    OldKeysForGameHost= NewKeysForGameHost;
                }
            }
        }

        
    }
}