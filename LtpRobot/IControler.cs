using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LtpRobot
{
    public interface IRobotControler
    {
        string[] AskBeforeRun { get; }
        void Execute(RobotManager robot, CancellationToken token, string[] args);
    }
}
