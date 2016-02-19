using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LtpRobot
{
    public class GameManager
    {
        public RobotClient Client;
        public Dictionary<int, RobotManager> Robots = new Dictionary<int, RobotManager>();
        public MapObserver renderer;
        private int CurrentRobotId = -1;
        private RobotManager CurrentRobot
        {
            get { return GetRobot(CurrentRobotId); }
        }

        public GameManager()
        {
            Commands = new Dictionary<string, Action<string[]>>()
            {
                { "dfs", p => SetRobotControler(CurrentRobotId, new DfsControler(), p) },
                {"reset", p =>
                {
                    Client.Reset();
                } },
                {"save", p => (p.Length == 0 ? CurrentRobot : GetRobot(int.Parse(p[0]))).Map.Save(p.FirstOrDefault() ?? CurrentRobotId.ToString()) },
                //{ "nsave", p=> (p.Length == 0 ? CurrentRobot : GetRobot(int.Parse(p[0]))).Map.NewSave(p.FirstOrDefault() ?? CurrentRobotId.ToString()) },
                {"load", p => CurrentRobot.Map.LoadMap(p[0]) },
                //{"nload", p => CurrentRobot.Map.NewLoadMap(p[0]) },
                //{"autoload", p=> CurrentRobot.Map.NewLoadMap(new DirectoryInfo(".").EnumerateFiles("*.robomap").OrderByDescending(f => f.LastWriteTime).First().FullName) },
                {"explore", p =>SetRobotControler(CurrentRobotId, new ExploreRobotControler(), p) },
                {"random", p => {
                    var len = int.Parse(p[0]);
                    var random = new Random();

                } },
                {"gohome", p => CurrentRobot.GoHome() },
                {"urandom", p =>
                {
                    while(true)
                    {
                        foreach (var i in CurrentRobot.Client.BatchExecute(CurrentRobot.RobotId, RandomStream(50000)))
                        { }
                    }
                } },
                {"image", p => CurrentRobot.Map.ExportPicture(p[0]) }
            };
        }

        public static ICollection<RobotAction> RandomStream(int limit)
        {
            var arr = new RobotAction[limit];
            var random = new Random();
            for (int i = 0; i < limit; i++)
            {
                switch (random.Next(3))
                {
                    case 0:
                        arr[i] = RobotAction.Left;
                        break;
                    case 1:
                        arr[i] = RobotAction.Right;
                        break;
                    case 2:
                        arr[i] = RobotAction.Go;
                        break;
                }
            }
            return arr;
        }

        public void SetMapObserve(int robotId)
        {
            var width = Console.BufferWidth;
            var height = Console.WindowHeight - 1;
            var robot = GetRobot(robotId);
            var posX = robot.Position.X - (width / 2);
            var posY = robot.Position.Y - (height / 2);
            if (renderer != null) renderer.Dispose();
            renderer = new MapObserver(robot.Map, posX, posY, width, height, 0, 0);
        }

        public void SetRobotBidning(int robotId)
        {
            var robot = GetRobot(robotId);
            if (renderer?.Map != robot.Map)
                SetMapObserve(robotId);
            renderer.SetRobotBind(robot);
            CurrentRobotId = robotId;
        }

        public void SetRobotLocation()
        {
            renderer.Robot = CurrentRobot;
            renderer.RefreshMapAndPosition();
            renderer.Robot = null;
        }

        public RobotManager GetRobot(int i)
        {
            if (Robots.ContainsKey(i)) return Robots[i];
            else return Robots[i] = InitRobot(i);
        }

        private RobotManager InitRobot(int i)
        {
            var r = new RobotManager(Client, i);
            return r;
        }

        public void SetRobotControler(int robotId, IRobotControler controler, string[] args)
        {
            cts = new CancellationTokenSource();
            Task.Run(() => controler.Execute(GetRobot(robotId), cts.Token, args))
                .ContinueWith(t =>
                {
                    if(t.Exception != null)
                    {

                    }
                });
        }
        private bool watcherRunning = false;
        public async void Watcher(RobotClient client)
        {
            while(watcherRunning)
            {
                var c1 = client.Counter;
                await Task.Delay(1000);
                Console.Title = "rps: " + (client.Counter - c1);
            }
        }

        public CancellationTokenSource cts = null;
        public Dictionary<string, Action<string[]>> Commands;


        public async void WaitForVoiceCommand()
        {
            var r = new System.Speech.Recognition.SpeechRecognizer();
        }

        public void ConsoleClient()
        {
            while (true)
            {
                try
                {
                    if (renderer != null)
                    {
                        renderer.Active = true;
                        renderer.Render();
                    }
                    var key = Console.ReadKey();
                    if (renderer != null) renderer.Active = false;
                    if (key.KeyChar == '=')
                    {
                        var ri = int.Parse(Console.ReadLine());
                        SetMapObserve(ri);
                    }
                    else if (key.KeyChar == '<')
                    {
                        var line = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            SetRobotLocation();
                        }
                        else
                        {
                            var ri = int.Parse(line);
                            SetRobotBidning(ri);
                        }
                    }
                    else if (key.KeyChar == ':')
                    {
                        var cmd = Console.ReadLine();
                        var s = cmd.Split(' ');
                        Commands[s[0]](s.Skip(1).ToArray());
                    }
                    else if (key.KeyChar == 'r')
                    {
                        var ri = int.Parse(Console.ReadLine());
                        if (renderer != null) renderer.Dispose();
                        //for (int ll = 0; ll < 50; ll++)
                        //{
                        //    Program.ss.Speak("Fuck!");
                        //}
                        renderer = null;
                        CurrentRobotId = ri;
                    }
                    else if (key.KeyChar == 'l')
                    {
                        var r = GetRobot(CurrentRobotId);
                        r.Rot360();
                    }
                    else if (key.Key == ConsoleKey.Escape)
                    {
                        if (cts != null) cts.Cancel();
                        cts = null;
                    }
                    else if(key.KeyChar == 'm')
                    {
                        watcherRunning = !watcherRunning;
                        Watcher(CurrentRobot.Client);
                    }
                    else if (key.KeyChar == 'w')
                        CurrentRobot.Move(RobotAction.Wait);
                    else if (key.KeyChar == 'c')
                        Console.Clear();
                    // move map
                    if (key.Key == ConsoleKey.UpArrow)
                        renderer.MapY--;
                    else if (key.Key == ConsoleKey.LeftArrow)
                        renderer.MapX--;
                    else if (key.Key == ConsoleKey.RightArrow)
                        renderer.MapX++;
                    else if (key.Key == ConsoleKey.DownArrow)
                        renderer.MapY++;
                    // move robot
                    // hex:
                    else if (key.Key == ConsoleKey.NumPad8)
                        CurrentRobot.GoTo(Rotation.Up);
                    else if (key.Key == ConsoleKey.NumPad2)
                        CurrentRobot.GoTo(Rotation.Down);
#if HEX
                    else if (key.Key == ConsoleKey.NumPad9)
                        CurrentRobot.GoTo(Rotation.RightUp);
                    else if (key.Key == ConsoleKey.NumPad6)
                        CurrentRobot.GoTo(Rotation.RightDown);
                    else if (key.Key == ConsoleKey.NumPad4)
                        CurrentRobot.GoTo(Rotation.LeftUp);
                    else if (key.Key == ConsoleKey.NumPad1)
                        CurrentRobot.GoTo(Rotation.LeftDown);
#else
                    else if (key.Key == ConsoleKey.NumPad4)
                        CurrentRobot.GoTo(Rotation.Left);
                    else if (key.Key == ConsoleKey.NumPad6)
                        CurrentRobot.GoTo(Rotation.Right);
#endif
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.ReadLine();
                    Console.WriteLine(e);
                    Console.ReadLine();
                    Console.WriteLine(e);
                    Console.ReadLine();
                }
            }
        }
    }
}
