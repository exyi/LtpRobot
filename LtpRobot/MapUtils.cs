using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LtpRobot
{
    public static class MapUtils
    {
        public static IEnumerable<Point> FindEdgeExplorePoints(this RobotMap map, Point from = default(Point))
        {
            var d = new Dictionary<Point, int>() { { from, 0 } };
            var q = new Queue<Point>(); q.Enqueue(from);

            while(true)
            {
                var p = FindFirstPoint(d, q, map, point => point.NearFour().Any(n => !map.Explored(n)));
                if (p.X == int.MaxValue) yield break;
                else yield return p;
            }
        }

        public static IEnumerable<Rotation> NearestNonExploredPointPath(this RobotMap map, Point from)
        {
            var d = new Dictionary<Point, int>() { { from, 0 } };
            var q = new Queue<Point>(); q.Enqueue(from);

            var p = FindFirstPoint(d, q, map, point => point.NearFour().Any(n => !map.Explored(n)));

            return GetPath(d, from, p);
        }

        private static Point FindFirstPoint(Dictionary<Point, int> d, Queue<Point> q, RobotMap map, Predicate<Point> goalPredicate)
        {
            while (q.Count > 0)
            {
                var e = q.Dequeue();
                var nf = e.NearFour();
                var i = d[e];
                i++;
                foreach (var n in nf)
                {
                    if (!d.ContainsKey(n) && map.Explored(n) && map[n].IsFree())
                    {
                        q.Enqueue(n);
                        d[n] = i;
                        if (goalPredicate(e))
                        {
                            return e;
                        }
                    }
                }
            }
            return new Point(int.MaxValue, int.MaxValue);
        }

        private static IEnumerable<Rotation> GetPath(Dictionary<Point, int> d, Point from, Point to)
        {
            var res = new List<Rotation>();
            while (!to.Equals(from))
            {
                var i = d[to] - 1;
                var nf = to.NearFour();
                int j;
                for (j = 0; j < nf.Length; j++)
                {
                    if (d.ContainsKey(nf[j]) && d[nf[j]] == i)
                    {
                        res.Add((Rotation)j);
                        to = nf[j];
                        break;
                    }
                }
                if (j == 4) throw new Exception();
            }
            var ress = res.ToArray();
            ReversePath(ress);
            return ress;
        }

        public static IEnumerable<Rotation> ShortestPaths(this RobotMap map, Point from, IEnumerable<Point> to)
        {
            var toHash = new HashSet<Point>(to);
            var d = new Dictionary<Point, int>(); d.Add(from, 0);
            var q = new Queue<Point>(); q.Enqueue(from);
            var c = FindFirstPoint(d, q, map, toHash.Remove);
            return GetPath(d, from, c);
        }

        public static bool IsFree(this MapTileResult mtr)
        {
            return mtr == MapTileResult.OpenDoor
                || mtr == MapTileResult.Free
                || mtr == MapTileResult.LeverA
                || mtr == MapTileResult.LeverB;
        }

        public static void ReversePath(Rotation[] path)
        {
            for (int i = 0; i < path.Length; i++)
            {
                path[i] = (Rotation)(((byte)path[i] + 2) % 4);
            }
            Array.Reverse(path);
        }
    }
}
