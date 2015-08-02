using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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
            writer.AutoFlush = true;
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

        public RobotActionResult Execute(int number, RobotAction action)
        {
            writer.Write(number + " " + (char)action + '\n');
            writer.Flush();
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

    public enum RobotAction: byte
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

    public enum MapTileResult: byte
    {
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
