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
        //public Dictionary<Point, MapTileResult> Map = new Dictionary<Point, MapTileResult>() { { new Point(0, 0), MapTileResult.Free } };

        public int OffsetX = 512;
        public int OffsetY = 512;
        public int Width = 1024;
        public int Height = 1024;
        public MapTileResult[] Array = new MapTileResult[1024 * 1024];

        public MapTileResult this[Point p]
        {
            get { return Array[p.X + OffsetX + (p.Y + OffsetY) * Width]; }
            set { EnsureSize(p.X, p.Y); Array[p.X + OffsetX + (p.Y + OffsetY) * Width] = value; }
        }

        public MapTileResult this[int x, int y]
        {
            get { return Array[x + OffsetX + (y + OffsetY) * Width]; }
            set { EnsureSize(x, y); Array[x + OffsetX + (y + OffsetY) * Width] = value; }
        }

        public bool EnsureSize(int x, int y)
        {
            int nOffX = OffsetX;
            int nOffY = OffsetY;
            while (x < -nOffX) nOffX *= 2;
            while (y < -nOffY) nOffY *= 2;
            var nWidth = Width - OffsetX + nOffX;
            var nHeight = Height - OffsetY + nOffY;
            while (x + nOffX >= nWidth) nWidth *= 2;
            while (y + nOffY >= nHeight) nHeight *= 2;

            if (nOffY == OffsetY && nOffX == OffsetX && nWidth == Width && nHeight == Height) return true;
            var narr = new MapTileResult[nWidth * nHeight];
            CopyTo(narr, nOffX, nOffY, nWidth, nHeight);
            Array = narr;
            OffsetX = nOffX;
            OffsetY = nOffY;
            Width = nWidth;
            Height = nHeight;
            return false;
        }

        private void CopyTo(MapTileResult[] arr, int offsetX, int offsetY, int width, int height)
        {
            int i = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var ax = (x - OffsetX) + offsetX;
                    var ay = (y - OffsetY) + offsetY;
                    if (ax >= 0 && ay >= 0 && ax < width && ay < height)
                    {
                        arr[ax + ay * width] = Array[i];
                    }

                    i++;
                }
            }
        }

        public bool TryGetTile(Point p, out MapTileResult tile)
        {
            var ax = p.X + OffsetX;
            var ay = p.Y + OffsetY;
            if (ay < 0 || ax < 0 || ay >= Height || ax >= Width)
            {
                tile = MapTileResult.Unknown; return false;
            }
            tile = Array[ax + ay * Width];
            return tile != MapTileResult.Unknown;
        }

        public bool Explored(Point p)
        {
            MapTileResult r;
            return TryGetTile(p, out r);
        }

        //public void AddMap(RobotMap map, Point offset)
        //{
        //    foreach (var item in map.Map)
        //    {
        //        Map.Add(item.Key + offset, item.Value);
        //    }
        //}

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
                    else if (Explored(p))
                    {
                        ch[x] = (char)this[p];
                    }
                    else ch[x] = ' ';
                }
                result[y] = new string(ch);
            }
            System.Array.Reverse(result);
            return result;
        }

        internal void RaiseOnChanged()
        {
            Changed?.Invoke();
        }

        public void Save(string name)
        {
            name = $"{ DateTime.Now.ToString("dd_MM_yy__hh.mm.ss") }xx{name}.txt";
            using (var writer = new StreamWriter(name))
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        MapTileResult r;
                        if(TryGetTile(new Point(x, y), out r))
                        {
                            writer.WriteLine($"[{x},{y}]:{(int)r}");
                        }
                    }
                }
            }
        }

        public void NewSave(string name)
        {
            name = $"{ DateTime.Now.ToString("dd_MM_yy__hh.mm.ss") }xx{name}.robomap";
            using (var file = File.OpenWrite(name))
            {
                using (var bw = new BinaryWriter(file, Encoding.ASCII, true))
                {
                    bw.Write(OffsetX);
                    bw.Write(OffsetY);
                    bw.Write(Width);
                    bw.Write(Height);
                }
                var buffer = new byte[Array.Length];
                Buffer.BlockCopy(Array, 0, buffer, 0, Array.Length);
                file.Write(buffer, 0, buffer.Length);
            }
        }

        public void NewLoadMap(string fileName)
        {
            using (var file = File.OpenRead(fileName))
            {
                using (var bw = new BinaryReader(file, Encoding.ASCII, true))
                {
                    OffsetX = bw.ReadInt32();
                    OffsetY = bw.ReadInt32();
                    Height = bw.ReadInt32();
                    Width = bw.ReadInt32();
                }
                var buffer = new byte[Width * Height];
                file.Read(buffer, 0, buffer.Length);
                Array = new MapTileResult[Width * Height];
                Buffer.BlockCopy(buffer, 0, Array, 0, buffer.Length);
            }
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
                    this[point] = (MapTileResult)(char)ch;
                }
            }
        }
    }
}
