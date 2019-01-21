using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Match3Lib
{
    class GameHintController
    {
        public Point[] GetHint(Match3Map gameMap)
        {
            var result = GetHorizontalHint(gameMap);
            if (result == null || result.Length < 2)
            {
                result = GetVerticalHint(gameMap);
            }
            return result;
        }

        static Point[] GetVerticalHint(Match3Map actualMap)
        {
            var result = GetHorizontalHint(actualMap.TransposeMap());
            return GetTransposedPoints(result);
        }
        static Point[] GetHorizontalHint(Match3Map actualMap)
        {
            for (int i = 0; i < Constants.checkingStep; i++)
            {
                for (int j = 0; j < Constants.checkingStep; j++)
                {
                    var mapCopy = new Match3Map(actualMap);

                    for (int x = i; x < mapCopy.Width - 1; x += Constants.checkingStep)
                    {
                        for (int y = j; y < mapCopy.Height; y += Constants.checkingStep)
                        {
                            actualMap.SwapCells(new Point(x, y), new Point(x + 1, y));
                        }
                    }

                    List<Point[]> bangLines = new List<Point[]>();
                    bangLines.AddRange(GetAllHorizontalBangLines(mapCopy));
                    bangLines.AddRange(GetAllVerticalBangLines(mapCopy));
                    if (bangLines.Count > 0)
                    {
                        mapCopy = CopyMap(actualMap);
                        for (int x = i; x < mapCopy.GetLength(0) - 1; x += checkingStep)
                        {
                            for (int y = j; y < mapCopy.GetLength(1); y += checkingStep)
                            {
                                SwapCells(new Point(x, y), new Point(x + 1, y), null, -1, ref mapCopy);
                                bangLines = new List<Point[]>();
                                bangLines.AddRange(GetAllHorizontalBangLines(mapCopy));
                                bangLines.AddRange(GetAllVerticalBangLines(mapCopy));
                                if (bangLines.Count > 0)
                                {
                                    return new Point[] { new Point(x, y), new Point(x + 1, y) };
                                }
                                else
                                {
                                    SwapCells(new Point(x, y), new Point(x + 1, y), null, -1, ref mapCopy);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        static void MakeAllSteps()
        {
 
        }

        static List<Point[]> GetAllVerticalBangLines(Match3MapItem[,] actualMap)
        {
            var submap = TransposeMap(actualMap);
            var result = GetAllHorizontalBangLines(submap);
            return result.Select(bangLine => GetTransposedPoints(bangLine)).ToList();
        }
        static List<Point[]> GetAllHorizontalBangLines(Match3MapItem[,] actualMap)
        {
            var result = new List<Point[]>();
            for (int i = 0; i < actualMap.GetLength(0) - 2; i++)
            {
                for (int j = 0; j < actualMap.GetLength(1); j++)
                {
                    if (actualMap[i, j] == null || actualMap[i, j].IsBlocked)
                    {
                        continue;
                    }
                    var startPointType = actualMap[i, j].ItemType;
                    int lineLength = 0;
                    while (actualMap.GetLength(0) > i + lineLength && actualMap[i + lineLength, j].ItemType == startPointType)
                    {
                        lineLength++;
                    }
                    if (lineLength > 2)
                    {
                        var bangLine = new Point[lineLength];
                        for (int k = 0; k < lineLength; k++)
                        {
                            bangLine[k] = new Point(i + k, j);
                        }
                        result.Add(bangLine);
                    }
                }
            }
            return result;

        }
           
        static Point[] GetTransposedPoints(Point[] points)
        {
            return points == null ? null : points.Select(item => new Point(item.y, item.x)).ToArray();
        }
    }
}
