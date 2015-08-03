using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LtpRobot
{
    public struct Point : IEquatable<Point>
    {
        public int X;
        public int Y;

        public static Point Add(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point Subtract(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }
        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }


        public Point Move(int rotation, int distance = 1)
        {
            switch ((Rotation)(rotation % 4))
            {
                case Rotation.Up:
                    return new Point(X, Y + distance);
                case Rotation.Right:
                    return new Point(X + distance, Y);
                case Rotation.Down:
                    return new Point(X, Y - distance);
                case Rotation.Left:
                    return new Point(X - distance, Y);
                default:
                    throw new Exception("WTF");
            }
        }

        public Point[] NearFour()
        {
            return new[]
            {
                new Point(X, Y + 1),
                new Point(X + 1, Y),
                new Point(X, Y - 1),
                new Point(X - 1, Y)
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Point))
            {
                return false;
            }
            return Equals((Point)obj);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return (X * 17) ^ (Y * 3);
        }

        public bool Equals(Point other)
        {
            return other.X == X & other.Y == Y;
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
