using System.Collections.Generic;

namespace Match3Lib
{
    internal static class Match3MapManager
    {

        public static void ResetMap(Match3Map actualMap, int cellTypesCount)
        {
            for (int i = 0; i < actualMap.Width; i++)
                for (int j = 0; j < actualMap.Height; j++)
                    actualMap.DestroyCell(new Point(i, j));

            FillAllEmptyCells(actualMap, cellTypesCount);
        }

        public static void DestroyStepCells(
            Match3Map actualMap
          , List<Point[]> bangLines
          , List<Point> comboCells
            )
        {
            for (int i = 0; i < bangLines.Count; i++)
                for (int j = 0; j < bangLines[i].Length; j++)
                    actualMap.DestroyCell(bangLines[i][j]);

            for (int i = 0; i < comboCells.Count; i++)
                actualMap.DestroyCell(comboCells[i]);
        }

        public static void MoveDownAllPossibleCells(Match3Map actualMap)
        {
            int swaps = 0;

            do
            {
                swaps = 0;
                for (int j = 0; j < actualMap.Height - 1; j++)
                    for (int i = 0; i < actualMap.Width; i++)
                        if (actualMap[i, j] == null && actualMap[i, j + 1] != null)
                        {
                            actualMap.SwapCells(new Point(i, j), new Point(i, j + 1));
                            swaps++;
                        }
            }
            while (swaps != 0);
        }

        public static void FillAllEmptyCells(Match3Map actualMap, int cellTypesCount)
        {
            for (int i = 0; i < actualMap.Width; i++)
                for (int j = actualMap.Height - 1; j > -1; j--)
                    if (actualMap[i, j] == null)
                        actualMap.CreateCell(new Point(i, j), Utils.GetRandomValue(cellTypesCount - 1));
        }
    }
}