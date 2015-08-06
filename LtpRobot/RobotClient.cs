using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LtpRobot
{
    public class RobotClient
    {
        string host;
        int port;
        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private TcpClient tcp;
        public int Counter = 0;
        bool dummyWait = false;

        public static RobotClient Connect(string host, int port)
        {
            return new RobotClient(host, port);
        }

        public RobotClient(TcpClient tcpClient)
        {
        }

        public RobotClient(string host, int port)
        {
            this.host = host;
            this.port = port;
            Init();
        }

        private void Init()
        {
            tcp = new TcpClient(host, port);
            stream = tcp.GetStream();
            writer = new StreamWriter(stream, Encoding.ASCII, 1);
            reader = new StreamReader(stream);
        }

        internal void Reset()
        {

        }

        public void Init(string userName, string token)
        {
            writer.Write(userName + '\n');
            writer.Write(token + '\n');
            writer.Flush();
        }

        public IEnumerable<RobotActionResult> BatchExecute(int ri, params RobotAction[] actions)
            => BatchExecute(ri, (ICollection<RobotAction>)actions);

        public IEnumerable<RobotActionResult> BatchExecute(int ri, ICollection<RobotAction> actions)
        {
            if (actions.Count <= 3)
            {
                foreach (var a in actions)
                {
                    writer.Write(ri + " " + (char)a + '\n');
                    writer.Flush();
                }
                if (dummyWait) { reader.ReadLine(); dummyWait = false; }
                for (int i = 0; i < actions.Count; i++)
                {
                    Interlocked.Increment(ref Counter);
                    yield return Parse(reader.ReadLine());
                }
            }
            else
            {
                int count = 0;
                foreach (var a in actions)
                {
                    count++;
                    writer.Write(ri + " " + (char)a + '\n');
                }
                writer.Flush();
                if (dummyWait) reader.ReadLine();
                writer.Write(ri + " w" + '\n');
                dummyWait = true;
                Interlocked.Increment(ref Counter);
                yield return Parse(reader.ReadLine());
                writer.Flush();
                for (int i = 1; i < count; i++)
                {
                    Interlocked.Increment(ref Counter);
                    yield return Parse(reader.ReadLine());
                }
            }
        }

        public RobotActionResult Execute(int number, RobotAction action)
        {
            writer.Write(number + " " + (char)action + '\n');
            writer.Flush();
            if (dummyWait) { reader.ReadLine(); dummyWait = false; }
            Interlocked.Increment(ref Counter);
            return Parse(reader.ReadLine());
        }

        public static RobotActionResult Parse(string result)
        {
            var r = (RobotResult)result[0];
            switch (r)
            {
                case RobotResult.Ok:
                case RobotResult.GoDenied:
                    return new RobotActionResult()
                    {
                        Result = r,
                        LeftMapTile = (MapTileResult)result[1],
                        FrontMapTile = (MapTileResult)result[2],
                        RightMapTile = (MapTileResult)result[3]
                    };
                case RobotResult.Goal:
                    return new RobotActionResult()
                    {
                        Result = RobotResult.Goal
                    };
                case RobotResult.Error:
                    throw new Exception("" + result);

                default:
                    throw new NotSupportedException();
            }
        }
    }

    public struct RobotActionResult
    {
        public RobotResult Result;
        public MapTileResult LeftMapTile;
        public MapTileResult FrontMapTile;
        public MapTileResult RightMapTile;
    }

    public enum RobotAction : byte
    {
        Right = (byte)'r',
        Left = (byte)'l',
        Wait = (byte)'w',
        Go = (byte)'g'
    }

    public enum RobotResult
    {
        Ok = (byte)'+',
        GoDenied = (byte)'#',
        Goal = (byte)'$',
        Error = (byte)'-'
    }

    public enum MapTileResult : byte
    {
        Unknown = 0,
        Free = (byte)'.',
        Wall = (byte)'#',
        Goal = (byte)'$',
        Robot = (byte)'@',
        ClosedDoor = (byte)'+',
        OpenDoor = (byte)'_',
        LeverA = (byte)'/',
        LeverB = (byte)'\\'
    }
}
