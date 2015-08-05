using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LtpRobot
{
    public class DfsControler : IRobotControler
    {
        public string[] AskBeforeRun => new string[] { };

        public void Execute(RobotManager robot, CancellationToken token, string[] args)
        {
            DoDfs(robot, token, int.Parse(args.FirstOrDefault() ?? int.MaxValue.ToString()));
        }

        public bool NextStep(RobotManager robot, RobotResult result, int limit)
        {
            if (robot.Position.NearFour().Any(n => !robot.Map.Explored(n)))
            {
                robot.Move(RobotAction.Left);
                return true;
            }

            return GetNextStepPoint(robot, limit);
        }

        private bool GetNextStepPoint(RobotManager robot, int limit)
        {
            var a = robot.Position.NearFour();
            var mi = -1;
            var max = 0;
            for (int i = 0; i < a.Length; i++)
            {
                var p = GetPriority(robot.Map, a[i], i);
                if (p >= max)
                {
                    max = p;
                    mi = i;
                }
            }
            if (max <= 1 || robot.Path.Count >= limit /* ||
                (robot.Map.Explored(robot.Position) && robot.Map[robot.Position] == MapTileResult.OpenDoor)*/)
            {
                if (robot.Path.Count == 0) return false;
                StackUnroll(robot, limit);
            }
            else robot.GoTo((Rotation)mi);
            return true;
        }

        private void StackUnroll(RobotManager robot, int limit)
        {
            var pos = robot.Position;
            var count = 0;
            while (true)
            {
                if (robot.Path.Count - count < limit)
                {
                    foreach (var n in pos.NearFour())
                    {
                        MapTileResult mt;
                        if(!robot.Map.TryGetTile(n, out mt) || (mt.IsFree() &&
                            !n.NearFour().All(robot.Map.Explored)))
                        {
                            break;
                        }
                    }
                }
                if (count >= robot.Path.Count)
                {
                    break;
                }
                count++;
                pos = pos.Move((byte)robot.Path[robot.Path.Count - count].Rot180());
            }
            if (count == 1)
            {
                robot.GoTo(robot.Path[robot.Path.Count - 1].Rot180());
            }
            else
            {
                var newPath = robot.Path.Take(robot.Path.Count - count).ToList();
                robot.GoTo(pos, restorePath: false, flushOnNewData: false);
                robot.Path = newPath;
            }
        }

        private int GetPriority(RobotMap map, Point p, int rotation)
        {
            if (!map.Explored(p)) return 5;
            var t = map[p];
            if (t == MapTileResult.Goal) return 10000000;
            if (t == MapTileResult.Wall || t == MapTileResult.Robot) return 0;
            if (t == MapTileResult.ClosedDoor) return 0;
            var count = 1;
            if (!map.Explored(p.Move(rotation + 4))) count++;
            if (!map.Explored(p.Move(rotation + 5))) count++;
            if (!map.Explored(p.Move(rotation))) count++;
            if (!map.Explored(p.Move(rotation + 2))) count++;
            if (!map.Explored(p.Move(rotation + 1))) count++;
            return count;
        }

        public void DoDfs(RobotManager robot, CancellationToken token, int limit = int.MaxValue)
        {
            try
            {
                var r = robot.Move(RobotAction.Wait);
                while (!token.IsCancellationRequested && NextStep(robot, r, limit))
                {
                }
            }
            catch (MyCoolExceptionForStopingDfsExecution)
            {
                return;
            }
        }
    }
}
