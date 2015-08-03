using System;
using System.Collections.Generic;
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
                {"load", p => CurrentRobot.Map.LoadMap(p[0]) },
                {"explore", p =>SetRobotControler(CurrentRobotId, new ExploreRobotControler(), p) },
                {"random", p => {
                    var len = int.Parse(p[0]);
                    var random = new Random();
                    for (int i = 0; i < len; i++)
                    {
                    switch (random.Next(3))
                    {
                       case 0: CurrentRobot.Client.Execute(CurrentRobot.RobotId, RobotAction.Left);
                           break;
                       case 1: CurrentRobot.Client.Execute(CurrentRobot.RobotId, RobotAction.Right);
                           break;
                       case 2: CurrentRobot.Client.Execute(CurrentRobot.RobotId, RobotAction.Go);
                           break;
                    }
                    }
                } }
            };
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
            Task.Run(() => controler.Execute(GetRobot(robotId), cts.Token, args));
        }

        public CancellationTokenSource cts = null;
        public Dictionary<string, Action<string[]>> Commands;

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
                    else if (key.KeyChar == 'w')
                        CurrentRobot.Move(RobotAction.Wait);
                    else if (key.KeyChar == 'c')
                        Console.Clear();
                    if (renderer != null && renderer.Robot == null)
                    {
                        if (key.Key == ConsoleKey.UpArrow)
                            renderer.MapY--;
                        else if (key.Key == ConsoleKey.LeftArrow)
                            renderer.MapX--;
                        else if (key.Key == ConsoleKey.RightArrow)
                            renderer.MapX++;
                        else if (key.Key == ConsoleKey.DownArrow)
                            renderer.MapY++;
                        renderer.Render();
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.UpArrow)
                            CurrentRobot.GoTo(Rotation.Up);
                        else if (key.Key == ConsoleKey.LeftArrow)
                            CurrentRobot.GoTo(Rotation.Left);
                        else if (key.Key == ConsoleKey.RightArrow)
                            CurrentRobot.GoTo(Rotation.Right);
                        else if (key.Key == ConsoleKey.DownArrow)
                            CurrentRobot.GoTo(Rotation.Down);
                    }
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
