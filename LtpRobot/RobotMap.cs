using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace LtpRobot
{
    public class RobotMap
    {
        public event Action Changed;
        public Dictionary<Point, MapTileResult> Map = new Dictionary<Point, MapTileResult>() { { new Point(0, 0), MapTileResult.Free } };

        public MapTileResult this[Point p]
        {
            get { return Map[p]; }
            set { Map[p] = value; }
        }

        public MapTileResult this[int x, int y]
        {
            get { return Map[new Point(x, y)]; }
            set { Map[new Point(x, y)] = value; }
        }

        public bool Explored(Point p)
        {
            return Map.ContainsKey(p);
        }

        public bool TryGetTile(Point p, out MapTileResult r)
            => Map.TryGetValue(p, out r);

        public void AddMap(RobotMap map, Point offset)
        {
            foreach (var item in map.Map)
            {
                Map.Add(item.Key + offset, item.Value);
            }
        }

        public string[] PrintMap(int xMin, int yMin, int width, int heigth, Point robotPosition)
        {
            var robotX = robotPosition.X - xMin;
            var robotY = robotPosition.Y - yMin;
            var result = new string[heigth];
            for (int y = 0; y < heigth; y++)
            {
                var ch = new char[width];
                for (int x = 0; x < width; x++)
                {
                    var p = new Point(xMin + x, yMin + y);
                    if (robotX == x && robotY == y)
                    {
                        ch[x] = '&';
                    }
                    else if (Map.ContainsKey(p))
                    {
                        ch[x] = (char)Map[p];
                    }
                    else ch[x] = ' ';
                }
                result[y] = new string(ch);
            }
            Array.Reverse(result);
            return result;
        }

        internal void RaiseOnChanged()
        {
            Changed?.Invoke();
        }

        public void Save(string name)
        {
            name = $"{ DateTime.Now.ToString("dd_MM_yy__hh.mm.ss") }xx{name}.txt";
            var collection = Map.ToArray().Select(s => $"[{s.Key.X},{s.Key.Y}]:{(int)s.Value}");
            File.WriteAllLines(name, collection);
        }

        public void LoadMap(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) break;
                    var s = line.Split(':');
                    var p = s[0].Substring(1, s[0].Length - 2);
                    var ps = p.Split(',');
                    var point = new Point(int.Parse(ps[0]), int.Parse(ps[1]));
                    var ch = int.Parse(s[1]);
                    Map[point] = (MapTileResult)(char)ch;
                }
                Program.Say("file loaded");
            }
        }

        public void ExportPicture(string name)
        {
            var minX = -3000; /*Map.Keys.Min(m => m.X);*/
            var minY = -3000; /*Map.Keys.Min(m => m.Y);*/
            var maxX = 4000; /*Map.Keys.Max(m => m.X);*/
            var maxY = 4000; /*Map.Keys.Max(m => m.Y);*/
            var width = (maxX - minX);
            var height = (maxY - minY);
            var bitmap = new Bitmap(width, height) ;
            var i = 0;
            foreach (var item in Map)
            {
                var x = (item.Key.X - minX);
                var y = (item.Key.Y - minY);
                if (x < 0 || y < 0 || x >= width || y >= height) continue;
                Color c = Color.Green;
                switch (item.Value)
                {
                    case MapTileResult.Free:
                        c = Color.Yellow;
                        break;
                    case MapTileResult.Wall:
                        c = Color.DarkBlue;
                        break;
                    case MapTileResult.Goal:
                        c = Color.Gold;
                        break;
                    case MapTileResult.Robot:
                        break;
                    case MapTileResult.ClosedDoor:
                        break;
                    case MapTileResult.OpenDoor:
                        break;
                    case MapTileResult.LeverA:
                        break;
                    case MapTileResult.LeverB:
                        break;
                }
                bitmap.SetPixel(x, y, c);
                i++;
            }
            bitmap.Save(name);
        }
    }
}
