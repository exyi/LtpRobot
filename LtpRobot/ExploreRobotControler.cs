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
                if ((r.Position.GetHashCode() % 5 == 30)) r.GoHome();
                var from = r.Position;
                r.Flush();
                var path = m.NearestNonExploredPointPath(from);
                foreach (var rot in path)
                {
                    r.GoTo(rot);
                }
                r.Flush();
                r.Path.Clear();
                new DfsControler().Execute(r, token, new[] { "350" });
                //r.Map.Save(r.RobotId.ToString());
            }
        }
    }
}
