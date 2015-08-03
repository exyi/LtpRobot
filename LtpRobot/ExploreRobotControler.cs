using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LtpRobot
{
    class ExploreRobotControler : IRobotControler
    {
        public string[] AskBeforeRun => new string[] { };

        public void Execute(RobotManager robot, CancellationToken token, string[] args)
        {
            DoExplore(robot, token);
        }

        public void DoExplore(RobotManager r, CancellationToken token)
        {
            var m = r.Map;
            while (!token.IsCancellationRequested)
            {
                var from = (r.Position.GetHashCode() % 5 == 0) ? new Point(0, 0) : r.Position;
                var path = m.NearestNonExploredPointPath(from);
                foreach (var rot in path)
                {
                    r.GoTo(rot);
                }
                r.Path.Clear();
                new DfsControler().Execute(r, token, new[] { "500" });
                //r.Map.Save(r.RobotId.ToString());
            }
        }
    }
}
