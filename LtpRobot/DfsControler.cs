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

        public RobotAction[] NextStep(RobotManager robot, RobotResult result, int limit)
        {
            if (!robot.Map.Explored(robot.Position.Move((byte)robot.Rot + 2)))
                return new[] { RobotAction.Left };

            var r = (byte)GetNextStepPoint(robot, limit);
            r -= (byte)robot.Rot;
            r += 4;
            r %= 4;
            if (r == 0) return new[] { RobotAction.Go };
            if (r == 1) return new[] { RobotAction.Right, RobotAction.Go };
            if (r == 2) return new[] { RobotAction.Left, RobotAction.Left, RobotAction.Go };
            if (r == 3) return new[] { RobotAction.Left, RobotAction.Go };
            throw new Exception("WTF");
        }

        private Rotation GetNextStepPoint(RobotManager robot, int limit)
        {
            var a = robot.Position.NearFour();
            var mi = 2;
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
                if (robot.Path.Count == 0) throw new MyCoolExceptionForStopingDfsExecution();
                return (Rotation)(((byte)robot.Path.Last() + 2) % 4);
            }
            return (Rotation)mi;
        }

        private int GetPriority(RobotMap map, Point p, int rotation)
        {
            if (!map.Explored(p)) return 5;
            var t = map[p];
            if (t == MapTileResult.Goal) return 10000000;
            if (t == MapTileResult.Wall || t == MapTileResult.Robot) return 0;
            if (t == MapTileResult.ClosedDoor) return 0;
            var count = 1;
            if (!map.Explored(p.Move(rotation + 3))) count++;
            if (!map.Explored(p.Move(rotation))) count++;
            if (!map.Explored(p.Move(rotation + 1))) count++;
            return count;
        }

        public void DoDfs(RobotManager robot, CancellationToken token, int limit = int.MaxValue)
        {
            try
            {
                var r = robot.Move(RobotAction.Wait);
                while (!token.IsCancellationRequested)
                {
                    var s = NextStep(robot, r, limit);
                    foreach (var x in s) robot.Move(x);
                }
            }
            catch (MyCoolExceptionForStopingDfsExecution)
            {
                return;
            }
        }
    }
}
