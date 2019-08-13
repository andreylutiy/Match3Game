using System;

namespace Match3Lib
{
    public class Point
    {
        public int x;
        public int y;

        public Point(int newX, int newY)
        {
            this.x = newX;
            this.y = newY;
        }

        public bool Equals(Point point)
        {
            if (point == null)
                return false;

            return this.x == point.x && this.y == point.y;
        }

        public double Magnitude( )
        {
            var result = Math.Pow(x, 2) + Math.Pow(y, 2);
            result = Math.Pow(result, 0.5);

            return result; 
        }

        public static Point operator -(Point val0, Point val1)
        {
            var result = new Point(0, 0);
            result.x = val0.x - val1.x;
            result.y = val0.y - val1.y;

            return result;
        }

        public static Point operator +(Point val0, Point val1)
        {
            var result = new Point(0, 0);
            result.x = val0.x + val1.x;
            result.y = val0.y + val1.y;

            return result;
        }
    }
}