using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LtpRobot
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = Connect("rumpal");
            var gm = new GameManager();
            gm.Client = c;
            gm.ConsoleClient();
        }

        public static void ConsoleClient(RobotClient c)
        {
            Console.Write("robot id: ");
            var robotId = int.Parse(Console.ReadLine());
            while (true)
            {
                var key = Console.ReadKey();
                RobotAction action = RobotAction.Wait;
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        action = RobotAction.Go;
                        break;
                    case ConsoleKey.RightArrow:
                        action = RobotAction.Right;
                        break;
                    case ConsoleKey.LeftArrow:
                        action = RobotAction.Left;
                        break;
                    case ConsoleKey.Escape:
                        return;
                    case ConsoleKey.C:
                        ConsoleClient(c);
                        break;
                }
                Console.Clear();
                Console.Write(robotId + ": " + action.ToString());
                var r = c.Execute(robotId, action);
                Console.WriteLine(r.Result);
                Console.WriteLine(" " + (char)r.FrontMapTile);
                Console.WriteLine((char)r.LeftMapTile + " " + (char)r.RightMapTile);
            }
        }

        public static RobotClient Connect(string token, string userName = "exyi")
        {
            var c = RobotClient.Connect("duncan.upir.cz", 3333);
            c.Init(userName, token);
            return c;
        }
#if HEX
        public const int Level = 6;
#else
        public const int Level = 4;
#endif
    }

    public enum Rotation : byte
    {
#if HEX
        Up,
        RightUp,
        RightDown,
        Down,
        LeftDown,
        LeftUp,
#else
        Up,
        Right,
        Down,
        Left
#endif
    }

    //class DfsMap
    //{


    //    public void ConsoleClient(RobotClient c)
    //    {
    //        Console.Write("robot id: ");
    //        var robotId = int.Parse(Console.ReadLine());
    //        var r = c.Execute(robotId, RobotAction.Wait);
    //        SaveResults(r);
    //        while (true)
    //        {
    //            Console.Clear();
    //            PrintMap();
    //            var key = Console.ReadKey();
    //            LtpRobot.Rotation rot = Rotation.Down;
    //            switch (key.Key)
    //            {
    //                case ConsoleKey.UpArrow:
    //                    rot = Rotation.Up;
    //                    break;
    //                case ConsoleKey.RightArrow:
    //                    rot = Rotation.Right;
    //                    break;
    //                case ConsoleKey.LeftArrow:
    //                    rot = Rotation.Left;
    //                    break;
    //                case ConsoleKey.Escape:
    //                    return;
    //                case ConsoleKey.C:
    //                    ConsoleClient(c);
    //                    break;
    //            }
    //            GoTo(rot, c, robotId);
    //        }
    //    }



    //    public void PrintMap()
    //    {
    //        var minX = Map.Keys.Min(p => p.X);
    //        var minY = Map.Keys.Min(p => p.Y);
    //        var maxX = Map.Keys.Max(p => p.X);
    //        var maxY = Map.Keys.Max(p => p.Y);

    //        var sizeX = Console.BufferWidth - 1;
    //        var sizeY = Console.WindowHeight - 2;
    //        minX = Position.X - (sizeX / 2);
    //        maxX = Position.X + (sizeX / 2);
    //        minY = Position.Y - (sizeY / 2);
    //        maxY = Position.Y + (sizeY / 2);

    //        for (int y = maxY - minY; y >= 0; y--)
    //        {
    //            var ch = new char[maxX - minX + 1];
    //            for (int x = 0; x <= maxX - minX; x++)
    //            {
    //                var p = new Point(minX + x, minY + y);
    //                if (Position.Equals(p))
    //                {
    //                    ch[x] = '&';
    //                }
    //                else if (Map.ContainsKey(p))
    //                {
    //                    ch[x] = (char)Map[p];
    //                }
    //                else ch[x] = ' ';
    //            }
    //            Console.WriteLine(ch);
    //        }
    //    }
    //}
}
