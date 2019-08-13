using System;
using System.Linq;
using System.Collections.Generic;

namespace Match3Lib
{
    internal static class Match3BangController
    {
        public static List<Point[]> GetAllBangLines(Match3Map actualMap)
        {
            var bangLines = new List<Point[]>();
            bangLines.AddRange(GetAllHorizontalBangLines(actualMap));
            bangLines.AddRange(GetAllVerticalBangLines(actualMap));

            return bangLines;
        }

        public static List<Point> GetComboBangItems(Match3Map actualMap, List<Point[]> bangLines)
        {
            var result = new List<Point>();
            result.AddRange(GetCellColumnComboItems(actualMap, bangLines, Constants.lineComboItemsLenght));
            result.AddRange(GetCellLineComboItems(actualMap, bangLines, Constants.lineComboItemsLenght));
            result.AddRange(GetCellTypeComboItems(actualMap, bangLines, Constants.cellTypeComboItemsLenght));
            result = result.Distinct().ToList();

            return result;
        }

        static List<Point[]> GetAllVerticalBangLines(Match3Map actualMap)
        {
            var result = GetAllHorizontalBangLines(actualMap.TransposeMap());

            return result
                .Select(bangLine => Utils.GetPointsForTransposedMatrix(bangLine))
                .ToList();
        }

        static List<Point[]> GetAllHorizontalBangLines(Match3Map actualMap)
        {
            var result = new List<Point[]>();

            for (int i = 0; i < actualMap.Width - 2; i++)
                for (int j = 0; j < actualMap.Height; j++)
                {
                    if (actualMap[i, j] == null || actualMap[i, j].IsBlocked)
                        continue;

                    var startPointType = actualMap[i, j].ItemType;
                    int lineLength = 0;

                    while (
                        actualMap.Width > i + lineLength
                     && actualMap[i + lineLength, j].ItemType == startPointType
                        )
                    {
                        lineLength++;
                    }

                    if (lineLength < 3)
                        continue;

                    var bangLine = new Point[lineLength];

                    for (int k = 0; k < lineLength; k++)
                        bangLine[k] = new Point(i + k, j);

                    result.Add(bangLine);
                }

            return result;
        }

        static List<Point> GetCellTypeComboItems(
            Match3Map actualMap
          , List<Point[]> bangLines
          , int targetBangLineLength
            )
        {
            var actualBangLines = bangLines
                .Where(line => line.Length == targetBangLineLength)
                .ToArray();

            var result = new List<Point>();

            for (int i = 0; i < actualBangLines.Count(); i++)
            {
                var bangLineCellsType = GetBangLineCellsType(actualMap, actualBangLines[i]);
                var possibleCells = actualMap.GetCellIndexesByType(bangLineCellsType);

                possibleCells = possibleCells
                    .Where(cell => !actualBangLines[i].Contains(cell))
                    .ToList();

                result.AddRange(possibleCells);
            }

            return result;
        }

        static List<Point> GetCellColumnComboItems(Match3Map actualMap, List<Point[]> bangLines, int targetBangLineLength)
        {
            var transposedMap = actualMap.TransposeMap();
            var transposedBangLines = bangLines
                                        .Select(line => Utils.GetPointsForTransposedMatrix(line))
                                        .ToList();

            var result = GetCellLineComboItems(transposedMap, transposedBangLines, targetBangLineLength);

            return Utils.GetPointsForTransposedMatrix(result);
        }

        static List<Point> GetCellLineComboItems(Match3Map actualMap, List<Point[]> bangLines, int targetBangLineLength)
        {
            var actualBangLines = bangLines.Where(line => line.Length == targetBangLineLength).ToArray();
            var result = new List<Point>();

            for (int i = 0; i < actualBangLines.Count(); i++)
            {
                if (actualBangLines[i] == null || actualBangLines[i].Length < 2 || actualBangLines[i][0].y != actualBangLines[i][1].y)
                    continue;

                var bangLineY = GetBangLineCellsLine(actualMap, actualBangLines[i]);
                var possibleCells = new Point[actualMap.Width];

                for (int j = 0; j < possibleCells.Length; j++)
                    possibleCells[j] = new Point(j, bangLineY);

                possibleCells = possibleCells
                                    .Where(cell => !actualBangLines[i].Contains(cell))
                                    .ToArray();
                result.AddRange(possibleCells);
            }

            return result;
        }

        static int GetBangLineCellsType(Match3Map actualMap, Point[] bangLine)
        {
            var bangLineTypes = new int[bangLine.Length];

            for (int i = 0; i < bangLine.Length; i++)
                bangLineTypes[i] = actualMap[bangLine[i]].ItemType;

            var possibleTypes = bangLineTypes.Distinct().ToArray();

            if (possibleTypes.Length == 1)
                return possibleTypes[0];
            else
                throw new ArgumentException("Match3Lib.Match3BangController.GetBangLineCellsType exception: bang line is invalid!");
        }

        static int GetBangLineCellsLine(Match3Map actualMap, Point[] bangLine)
        {
            var bangLineLines = new int[bangLine.Length];

            for (int i = 0; i < bangLine.Length; i++)
                bangLineLines[i] = bangLine[i].y;

            var possibleLines = bangLineLines.Distinct().ToArray();

            if (possibleLines.Length == 1)
                return possibleLines[0];
            else
                throw new ArgumentException("Match3Lib.Match3BangController.GetBangLineCellsLine exception: bang line is invalid!");
        }
    }
}