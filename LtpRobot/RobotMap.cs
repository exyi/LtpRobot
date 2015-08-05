using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LtpRobot
{
    public class RobotMap
    {
        public event Action Changed;
        public Dictionary<Point, MapTileResult> Map = new Dictionary<Point, MapTileResult>() { { new Point(0, 0), MapTileResult.Free } };

        public MapTileResult this[Point p]
        {
            get { return Map[p]; }
            set { Map[p] = value; Changed?.Invoke(); }
        }

        public MapTileResult this[int x, int y]
        {
            get { return Map[new Point(x, y)]; }
            set { Map[new Point(x, y)] = value; Changed?.Invoke(); }
        }

        public bool TryGetTile(Point p, out MapTileResult tile)
            => Map.TryGetValue(p, out tile);

        public bool Explored(Point p)
        {
            return Map.ContainsKey(p);
        }

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
                    if(robotX == x && robotY == y)
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
                    if (string.IsNullOrWhiteSpace(line)) return;
                    var s = line.Split(':');
                    var p = s[0].Substring(1, s[0].Length - 2);
                    var ps = p.Split(',');
                    var point = new Point(int.Parse(ps[0]), int.Parse(ps[1]));
                    var ch = int.Parse(s[1]);
                    Map[point] = (MapTileResult)(char)ch;
                }
            }
        }
    }
}
