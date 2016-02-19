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
        private bool flushing = true;
        private bool flushOnEnd = false;

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

        public void GoTo(Rotation rot, bool raiseEvents = true)
        {
            Move(rot.ToActions(Rot), raiseEvents);
        }

        private RobotAction[] Expand(RobotAction action)
        {
            MapTileResult r;
            if (action == RobotAction.Go && Map.TryGetTile(Position, out r) && (r == MapTileResult.LeverA || r == MapTileResult.LeverB))
            {
                // reshitch lever
                return new[] {
                        action,
                        RobotAction.Left,
                        RobotAction.Left,
                        RobotAction.Left,
                        RobotAction.Go,
                        RobotAction.Left,
                        RobotAction.Left,
                        RobotAction.Left,
                        RobotAction.Go };
            }
            else return new[] { action };
        }

        public RobotResult Move(RobotAction action, bool raiseEvents = true)
        {
            RobotResult r = RobotResult.Ok;
            if (action == RobotAction.Go)
            {
                if (Path.Count != 0 && Path.Last() == Rot.Rot180())
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
            if (raiseEvents)
            {
                Map.RaiseOnChanged();
                Moved?.Invoke();
            }
            return r;
        }

        public void Move(IEnumerable<RobotAction> actions, bool raiseEvents = true)
        {
            foreach (var action in actions)
            {
                if (action == RobotAction.Go)
                {
                    if (Path.Count != 0 && Path.Last() == Rot.Rot180())
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
                    MoveBuf(a);
                }
            }
            if (!Map.Explored(Position) || !Position.NearFour().All(Map.Explored))
                Flush(false);
            if (raiseEvents)
            {
                Map.RaiseOnChanged();
                Moved?.Invoke();
            }
        }

        private RobotResult MoveInternal(RobotAction action)
        {
            if (action != RobotAction.Go
                && Position.NearFour().All(Map.Explored))
            {
                MoveBuf(action);
                return RobotResult.Ok;
            }
            var p = Position.Move((int)Rot);
            if (Map.Explored(p) && action == RobotAction.Go)
            {
                var t = Map[p];
                if (t.IsFree()
                    && t != MapTileResult.OpenDoor
                    && p.NearFour().All(Map.Explored))
                {
                    MoveBuf(RobotAction.Go);
                    return RobotResult.Ok;
                }
                else if (t == MapTileResult.Wall)
                {
                    return RobotResult.GoDenied;
                }
            }

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

        public void GoTo(Point p, bool restorePath = true, bool flushOnNewData = true)
        {
            //Flush();
            var path = Map.ShortestPath(Position, new[] { p });
            flushing = false;
            foreach (var item in path)
            {
                GoTo(item, false);
            }
            flushing = true;
            if (flushOnEnd && flushOnNewData)
            {
                flushOnEnd = false;
                Flush();
            }
            if (restorePath)
            {
                var pathFromHome = Map.ShortestPath(new Point(0, 0), new[] { p });
                Path = pathFromHome.ToList();
            }
            Map.RaiseOnChanged();
            Moved?.Invoke();
        }

        private void MoveBuf(RobotAction action)
        {
            if (BufferLocation == null) BufferLocation = new BufInfo() { Position = Position, Rot = Rot };
            BufferLocation.Actions.Add(action);
            MoveResult(action);
        }

        public void Flush(bool throwOnWtf = true)
        {
            if (!flushing)
            {
                flushOnEnd = true;
                return;
            }
            if (BufferLocation == null) return;
            Position = BufferLocation.Position;
            Rot = BufferLocation.Rot;
            int i = 0;
            foreach (var r in Client.BatchExecute(RobotId, BufferLocation.Actions))
            {
                if (r.Result != RobotResult.Ok) { if (throwOnWtf)
                    {
                        Program.ss.Speak(string.Join(" ", Enumerable.Repeat("Fuck!", 50)));
                        throw new Exception("FUCK");
                    }
                }
                else
                {
                    MoveResult(BufferLocation.Actions[i++]);
                    SaveResults(r);
                }
            }
            BufferLocation = null;
        }

        private void MoveResult(RobotAction action)
        {
            if (action == RobotAction.Left)
            {
                Rot = (Rotation)(((byte)Rot + 5) % 6);
            }
            else if (action == RobotAction.Right)
            {
                Rot = (Rotation)(((byte)Rot + 1) % 6);
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
            SaveResult(r + 5, result.LeftMapTile);
            SaveResult(r, result.FrontMapTile);
            SaveResult(r + 1, result.RightMapTile);
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
            Map[Position.Move(rot)] = tile;
        }

        public void Rot360()
        {
            for (int i = 0; i < 6; i++)
            {
                Move(RobotAction.Left);
            }
        }
    }
}
