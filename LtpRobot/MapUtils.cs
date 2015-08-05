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
                    MapTileResult r;
                    if (!d.ContainsKey(n) && map.TryGetTile(n, out r) && r.IsFree())
                    {
                        q.Enqueue(n);
                        d[n] = i;
                        if (goalPredicate(n))
                        {
                            return n;
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
                if (j == nf.Length) throw new Exception();
            }
            var ress = res.ToArray();
            ReversePath(ress);
            return ress;
        }

        public static IEnumerable<Rotation> ShortestPath(this RobotMap map, Point from, IEnumerable<Point> to)
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
                path[i] = (Rotation)(((byte)path[i] + 3) % 6);
            }
            Array.Reverse(path);
        }

        public static Rotation Rot180(this Rotation r)
        {
#if HEX
            return (Rotation)(((byte)r + 3) % 6);
#else
            return (Rotation)(((byte)r + 2) % 4);
#endif
        }

        public static RobotAction[] ToActions(this Rotation rot, Rotation current = Rotation.Up)
        {
            var r = (int)rot;
            r = (r - (int)current + Program.Level) % Program.Level;

            if (r == 0) return new[] { RobotAction.Go };
            if (r == 1) return new[] { RobotAction.Right, RobotAction.Go };
            if (r == 2) return new[] { RobotAction.Right, RobotAction.Right, RobotAction.Go };
            // down
            if (r == 3) return new[] { RobotAction.Left, RobotAction.Left, RobotAction.Left, RobotAction.Go };
            if (r == 4) return new[] { RobotAction.Left, RobotAction.Left, RobotAction.Go };
            if (r == 5) return new[] { RobotAction.Left, RobotAction.Go };
            throw new Exception("WTF???");
        }
    }
}
