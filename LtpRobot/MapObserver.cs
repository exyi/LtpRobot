using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LtpRobot
{
    public class MapObserver : IDisposable
    {
        public int ConsoleX;
        public int ConsoleY;
        public int MapX;
        public int MapY;
        public int Width;
        public int Height;
        public RobotMap Map;
        public RobotManager Robot;
        public bool Active = true;

        public MapObserver(RobotMap map, int mapX, int mapY, int width, int height, int consoleX, int consoleY)
        {
            Map = map;
            MapX = mapX;
            MapY = mapY;
            Width = width;
            Height = height;
            ConsoleX = consoleX;
            ConsoleY = consoleY;
            Observe();
            Render();
        }

        public void Observe()
        {
            Map.Changed += Render;
            if (Robot != null) Robot.Moved += Render;
        }

        public void Dispose()
        {
            Map.Changed -= Render;
        }

        public void Render()
        {
            if (Active)
            {
                var r = Map.PrintMap(MapX, MapY, Width, Height, Robot == null ? new Point(int.MaxValue, int.MaxValue) : Robot.Position);
                for (int i = 0; i < r.Length; i++)
                {
                    Console.CursorLeft = ConsoleX;
                    Console.CursorTop = ConsoleY + i;
                    Console.Write(r[i]);
                }
            }
        }

        public void RefreshMapAndPosition()
        {
            MapX = Robot.Position.X - (Width / 2);
            MapY = Robot.Position.Y - (Height / 2);
            Render();
        }

        public void SetRobotBind(RobotManager robot)
        {
            Robot = robot;
            Robot.Moved += RefreshMapAndPosition;
        }
    }
}
