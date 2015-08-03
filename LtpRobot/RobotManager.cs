using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LtpRobot
{
    public class RobotManager
    {
        public RobotClient Client;
        public int RobotId;
        public RobotMap Map;
        public Point Position;
        public Rotation Rot;
        public List<Rotation> Path = new List<Rotation>();
        private BufInfo BufferLocation = null;
        public bool DoorAlwaysOpen = true;

        public event Action Moved;

        class BufInfo
        {
            public Rotation Rot;
            public Point Position;
            public List<RobotAction> Actions = new List<RobotAction>();
        }

        public RobotManager(RobotClient client, int robotId)
        {
            Client = client;
            RobotId = robotId;
            Map = new RobotMap();
        }

        public void GoTo(Rotation rot)
        {
            RobotAction action = RobotAction.Wait;
            while (Rot != rot)
            {
                if (((byte)rot - (byte)(Rot) + 4) % 4 == 1)
                {
                    action = RobotAction.Right;
                }
                else
                {
                    action = RobotAction.Left;
                }
                Move(action);
            }
            Move(RobotAction.Go);
        }

        private RobotAction[] Expand(RobotAction action)
        {
            if (action == RobotAction.Go && (!Map.Explored(Position) || Map[Position] == MapTileResult.LeverA || Map[Position] == MapTileResult.LeverB))
            {
                // reshitch lever
                return new[] {
                        action,
                        RobotAction.Left,
                        RobotAction.Left,
                        RobotAction.Go,
                        RobotAction.Left,
                        RobotAction.Left,
                        RobotAction.Go };
            }
            else return new[] { action };
        }

        public RobotResult Move(RobotAction action)
        {
            RobotResult r = RobotResult.Ok;
            if (action == RobotAction.Go)
            {
                if (Path.Count != 0 && Path.Last() == (Rotation)(((byte)Rot + 2) % 4))
                {
                    Path.RemoveAt(Path.Count - 1);
                }
                else
                {
                    Path.Add(Rot);
                }
            }
            foreach (var a in Expand(action))
            {
                r = MoveInternal(a);
            }
            Moved?.Invoke();
            return r;
        }

        private RobotResult MoveInternal(RobotAction action)
        {
            //if (action != RobotAction.Go
            //    && Map.Explored(Position.Move(0))
            //    && Map.Explored(Position.Move(1))
            //    && Map.Explored(Position.Move(2))
            //    && Map.Explored(Position.Move(3))
            //    )
            //{
            //    MoveBuf(action);
            //    return RobotResult.Ok;
            //}
            //var p = Position.Move((int)Rot);
            //if (Map.Explored(p) && action == RobotAction.Go)
            //{
            //    var t = Map[p];
            //    if (t.IsFree()
            //        && Map.Explored(p.Move((int)Rot + 3))
            //        && Map.Explored(p.Move((int)Rot))
            //        && Map.Explored(p.Move((int)Rot + 1))
            //        )
            //    {
            //        MoveBuf(RobotAction.Go);
            //        return RobotResult.Ok;
            //    }
            //    else if (t == MapTileResult.Wall)
            //    {
            //        return RobotResult.GoDenied;
            //    }
            //}

            Flush();
            var result = Client.Execute(RobotId, action);
            if (result.Result == RobotResult.Ok)
            {
                MoveResult(action);
            }
            SaveResults(result);
            return result.Result;
        }

        public void GoHome()
            => GoTo(new Point(0, 0));

        public void GoTo(Point p)
        {
            Flush();
            var path = Map.ShortestPaths(Position, new[] { p });
            foreach (var r in path)
            {
                GoTo(r);
            }
            var pathFromHome = Map.ShortestPaths(new Point(0, 0), new[] { p });
            Path = pathFromHome.ToList();
        }

        private void MoveBuf(RobotAction action)
        {
            if (BufferLocation == null) BufferLocation = new BufInfo() { Position = Position, Rot = Rot };
            BufferLocation.Actions.Add(action);
            MoveResult(action);
        }

        public void Flush()
        {
            if (BufferLocation == null) return;
            Position = BufferLocation.Position;
            Rot = BufferLocation.Rot;
            int i = 0;
            foreach (var r in Client.BatchExecute(RobotId, BufferLocation.Actions))
            {
                if (r.Result != RobotResult.Ok) throw new Exception("FUCK");
                MoveResult(BufferLocation.Actions[i++]);
            }
            BufferLocation = null;
        }

        private void MoveResult(RobotAction action)
        {
            if (action == RobotAction.Left)
            {
                Rot = (Rotation)(((byte)Rot + 3) % 4);
            }
            else if (action == RobotAction.Right)
            {
                Rot = (Rotation)(((byte)Rot + 1) % 4);
            }
            else if (action == RobotAction.Go)
            {
                Position = Position.Move((byte)Rot);
            }
        }

        public void SaveResults(RobotActionResult result)
        {
            if (result.Result != RobotResult.Ok && result.Result != RobotResult.GoDenied) return;
            byte r = (byte)Rot;
            SaveResult(r + 3, result.LeftMapTile);
            SaveResult(r, result.FrontMapTile);
            SaveResult(r + 1, result.RightMapTile);
            Map.RaiseOnChanged();
        }

        //public RobotActionResult MoveBuffered(RobotAction[] s)
        //{
        //    foreach (var si in s)
        //    {
        //        Move(s);
        //    }
        //}

        private void SaveResult(int rot, MapTileResult tile)
        {
            Map.Map[Position.Move(rot)] = tile;
        }

        public void Rot360()
        {
            for (int i = 0; i < 4; i++)
            {
                Move(RobotAction.Left);
            }
        }
    }
}
