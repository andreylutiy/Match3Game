using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Match3Lib
{
    class Match3GameStepController
    {
        public  static List<Point[]> GetAllBangLines(Match3Map actualMap)
        {
            var bangLines = new List<Point[]>();
            bangLines.AddRange(GetAllHorizontalBangLines(actualMap));
            bangLines.AddRange(GetAllVerticalBangLines(actualMap));
            return bangLines;
        }
        static List<Point[]> GetAllVerticalBangLines(Match3Map  actualMap)
        {
            var result = GetAllHorizontalBangLines(actualMap.TransposeMap());
            return result.Select(bangLine => GetTransposedPoints(bangLine)).ToList();
        }
        static List<Point[]> GetAllHorizontalBangLines(Match3Map actualMap)
        {
            var result = new List<Point[]>();
            for (int i = 0; i < actualMap.Width  - 2; i++)
            {
                for (int j = 0; j < actualMap.Height; j++)
                {
                    if (actualMap[i, j] == null || actualMap[i, j].IsBlocked)
                    {
                        continue;
                    }
                    var startPointType = actualMap[i, j].ItemType;
                    int lineLength = 0;
                    while (actualMap.Width > i + lineLength && actualMap[i + lineLength, j].ItemType == startPointType)
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
        static bool IsPointsValidToSwap(Point point0, Point point1, Point mapSize)
        {
            var result = true;
            result = result && Utils.IsCellInRange(point0, mapSize);
            result = result && Utils.IsCellInRange(point1, mapSize);
            result = result && (Math.Abs(point0.x - point1.x) + Math.Abs(point0.y - point1.y)) == 2;
            return result;
        }

    }
}
