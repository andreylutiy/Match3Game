using System;
using System.Collections.Generic;

namespace Match3Lib
{
    internal static class ScoreManager
    {
        public static int GetScore(List<Point[]> bangLines, List<Point> comboBangPoints)
        {
            var result = 0;

            for (int i = 0; i < bangLines.Count; i++)
                result += GetBangLineScore(bangLines[i]);

            result += GetComboBangPointsScore(comboBangPoints);
            return result;
        }

        static int GetBangLineScore(Point[] bangLine)
        {
            if (bangLine == null)
                return 0;

            var result = Math.Pow(bangLine.Length, 2);
            result = Math.Round(result);

            return (int)result;
        }

        static int GetComboBangPointsScore(List<Point> comboBangPoints)
        {
            if (comboBangPoints == null)
                return 0;

            return comboBangPoints.Count * 2;
        }
    }
}