using System.Collections.Generic;

namespace Match3Lib
{
    internal static class MapStateManager
    {
        public static bool IsMapValid(Match3Map gameMap)
        {
            return IsAnyVerticalStepsEffective(gameMap) || IsAnyHorizontalStepsEffective(gameMap);
        }

        static Point[] GetHint(Match3Map gameMap)
        {
            var hints = new List<Point[]>();
            hints.AddRange(GetHorizontalHints(gameMap));
            hints.AddRange(GetVerticalHints(gameMap));

            if (hints.Count > 0)
            {
                var resultIndex = Utils.GetRandomValue(hints.Count - 1);
                return hints[resultIndex];
            }

            return null;
        }

        static List<Point[]> GetVerticalHints(Match3Map actualMap)
        {
            var hints = GetHorizontalHints(actualMap.TransposeMap());
            var result = new List<Point[]>();

            for (int i = 0; i < hints.Count; i++)
                result.Add(Utils.GetPointsForTransposedMatrix(hints[i]));

            return result;
        }

        static List<Point[]> GetHorizontalHints(Match3Map actualMap)
        {
            var result = new List<Point[]>();

            for (int i = 0; i < Constants.checkingStep; i++)
                for (int j = 0; j < Constants.checkingStep; j++)
                {
                    var mapCopy = new Match3Map(actualMap);
                    MakeAllSteps(mapCopy, new Point(i, j));

                    var bangLines = Match3BangController.GetAllBangLines(mapCopy);

                    if (bangLines.Count > 0)
                        result.AddRange(GetHintsByStep(actualMap, new Point(i, j)));
                }

            return result;
        }

        static bool IsAnyVerticalStepsEffective(Match3Map actualMap)
        {
            return IsAnyHorizontalStepsEffective(actualMap.TransposeMap());
        }

        static bool IsAnyHorizontalStepsEffective(Match3Map actualMap)
        {
            for (int i = 0; i < Constants.checkingStep; i++)
                for (int j = 0; j < Constants.checkingStep; j++)
                {
                    var mapCopy = new Match3Map(actualMap);
                    MakeAllSteps(mapCopy, new Point(i, j));

                    var bangLines = Match3BangController.GetAllBangLines(mapCopy);

                    if (bangLines.Count > 0)
                        return true;

                }

            return false;
        }

        static void MakeAllSteps(Match3Map actualMap, Point startPoint)
        {

            for (int x = startPoint.x; x < actualMap.Width - 1; x += Constants.checkingStep)
                for (int y = startPoint.y; y < actualMap.Height; y += Constants.checkingStep)
                    actualMap.SwapCells(new Point(x, y), new Point(x + 1, y));
        }

        static List<Point[]> GetHintsByStep(Match3Map actualMap, Point startPoint)
        {
            var result = new List<Point[]>();

            for (int x = startPoint.x; x < actualMap.Width - 1; x += Constants.checkingStep)
                for (int y = startPoint.y; y < actualMap.Height; y += Constants.checkingStep)
                {
                    var mapCopy = new Match3Map(actualMap);
                    mapCopy.SwapCells(new Point(x, y), new Point(x + 1, y));

                    if (Match3BangController.GetAllBangLines(mapCopy).Count > 0)
                        result.Add(new Point[] { new Point(x, y), new Point(x + 1, y) });
                }

            return result;
        }
    }
}