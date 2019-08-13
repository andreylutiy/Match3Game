using System;
using System.Collections.Generic;
using System.Linq;

namespace Match3Lib
{
    internal static class Utils
    {
        static Random rand = new Random();

        public static int GetRandomValue(int maxValue)
        {
            return (int)Math.Round(maxValue * rand.NextDouble());
        }

        public static bool IsCellInRange(Point cell, Point range)
        {
            if (cell == null)
                return false;

            if (!Enumerable.Range(0, range.x).Contains(cell.x))
                return false;

            if (!Enumerable.Range(0, range.y).Contains(cell.y))
                return false;

            return true;
        }

        public static Point[] GetPointsForTransposedMatrix(Point[] points)
        {
            if (points == null)
                return null;

            return points
                        .Select(item => new Point(item.y, item.x))
                        .ToArray();
        }

        public static List<Point> GetPointsForTransposedMatrix(List<Point> points)
        {
            if (points == null)
                return null;

            return points
                        .Select(item => new Point(item.y, item.x))
                        .ToList();
        }
    }
}