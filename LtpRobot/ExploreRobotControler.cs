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
            Program.Say("Starting explore");
            var m = r.Map;
            while (!token.IsCancellationRequested)
            {
                if ((r.Position.GetHashCode() % 5 == 15)) r.GoHome();
                var from = r.Position;
                r.Flush();
                var nep = m.NearestNonExploredPoint(from);
                r.GoTo(nep, restorePath: false, flushOnNewData: false);
                r.Path.Clear();
                Program.Say($"Starting DFS on {r.Position.X}, {r.Position.Y}");
                new DfsControler().Execute(r, token, new[] { "350" });
                //r.Map.Save(r.RobotId.ToString());
            }
        }
    }
}
