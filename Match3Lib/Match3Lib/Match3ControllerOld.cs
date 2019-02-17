//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Match3Lib
//{
//    public class Match3ControllerOld
//    {
//        IGameEventsListener gameEventsListener;

//        static Random rand = new Random();
//        Match3MapItem[,] map;
//        int cellTypesCount;
//        public int ActualScore { get; private set; }
//        int currentStepIndex;


//        public void NewGame(int xSize, int ySize, IGameEventsListener actualGameEventsListener, int actualCellTypesCount = 3)
//        {
//            if (xSize < Constants.minSize || xSize > Constants.maxSize || ySize < Constants.minSize || ySize > Constants.maxSize)
//            {
//                throw new ArgumentOutOfRangeException("Match3Lib.Match3Controller.Match3Map exception: map size out of range!");
//            }
//            if (actualCellTypesCount < 2)
//            {
//                throw new ArgumentOutOfRangeException("Match3Lib.Match3Controller.Match3Map exception: cellTypesCount out of range!");
//            }
//            map = new Match3MapItem[xSize, ySize];
//            cellTypesCount = actualCellTypesCount;
//            currentStepIndex = 0;
//            gameEventsListener = actualGameEventsListener;
//            ActualScore = UpdateMapState();
//        }
//        public bool MakeStep(Point cell0, Point cell1)
//        {
//            int addScore = 0;
//            if (!(IsPointValid(cell0, map.GetLength(0), map.GetLength(1)) && IsPointValid(cell1, map.GetLength(0), map.GetLength(1)) && (Math.Abs(cell0.x - cell1.x) + Math.Abs(cell0.y - cell1.y)) < 2))
//            {
//                return false;
//            }
//            SwapCells(cell0, cell1, gameEventsListener, currentStepIndex, ref map);
//            addScore = UpdateMapState();
//            if (addScore > 0)
//            {
//                ActualScore += addScore;
//                gameEventsListener.OnScore(ActualScore, currentStepIndex);
//                return true;
//            }
//            else
//            {
//                SwapCells(cell0, cell1, gameEventsListener, currentStepIndex, ref map);
//                return false;
//            }
//        }
//        public Point[] GetHint()
//        {
//            var result = GetHorizontalHint(map);
//            if (result == null || result.Length < 2)
//            {
//                result = GetVerticalHint(map);
//            }
//            return result;
//        }

//        bool IsMapValid()
//        {
//            var result = GetHint();
//            return result != null && result.Length > 1;
//        }

//        int UpdateMapState()
//        {
//            int result = 0;
//            bool isCurrentMapValid = true;
//            do
//            {
//                if (!isCurrentMapValid)
//                {
//                    for (int i = 0; i < map.GetLength(0); i++)
//                    {
//                        for (int j = 0; j < map.GetLength(1); j++)
//                        {
//                            gameEventsListener.OnCellDestroy(i, j, currentStepIndex);
//                            map[i, j] = null;
//                        }
//                    }
//                }

//                int previosResult = 0;
//                do
//                {
//                    previosResult = result + 0;
//                    CreateNewItems();
//                    currentStepIndex++;
//                    result += DestroyAllBangLines();
//                    gameEventsListener.OnScore(ActualScore + result, currentStepIndex);
//                }
//                while (result != previosResult);

//                isCurrentMapValid = IsMapValid();
//            }
//            while (!isCurrentMapValid);

//            return result;
//        }
//        int DestroyAllBangLines()
//        {
//            List<Point[]> bangLines = new List<Point[]>();
//            bangLines.AddRange(GetAllHorizontalBangLines(map));
//            bangLines.AddRange(GetAllVerticalBangLines(map));

//            int result = 0;
//            var itemTypesForDestroy = bangLines.Where(line => line.Length >= 5).Select(line => map[line[0].x, line[0].y].ItemType).ToArray();
//            var itemLinesForDestroy = bangLines.Where(line => line.Length == 4).Select(line => line[0].x == line[1].x ? line[1].x : -1).ToArray();
//            var itemColumnsForDestroy = bangLines.Where(line => line.Length == 4).Select(line => line[0].y == line[1].y ? line[1].y : -1).ToArray();
//            for (int i = 0; i < bangLines.Count; i++)
//            {
//                result += (int)Math.Pow(bangLines[i].Length, 2);
//                for (int j = 0; j < bangLines[i].Length; j++)
//                {
//                    gameEventsListener.OnCellDestroy(bangLines[i][j].x, bangLines[i][j].y, currentStepIndex);
//                    map[bangLines[i][j].x, bangLines[i][j].y] = null;
//                }
//            }

//            for (int x = 0; x < map.GetLength(0); x++)
//            {
//                for (int y = 0; y < map.GetLength(1); y++)
//                {
//                    if (map[x, y] != null)
//                    {
//                        if (itemTypesForDestroy.Contains(map[x, y].ItemType) || itemLinesForDestroy.Contains(x) || itemColumnsForDestroy.Contains(y))
//                        {
//                            result++;
//                            gameEventsListener.OnCellDestroy(x, y, currentStepIndex);
//                            map[x, y] = null;
//                        }
//                    }
//                }
//            }


//            int swaps = 0;
//            do
//            {
//                swaps = 0;
//                for (int j = 0; j < map.GetLength(1) - 1; j++)
//                {
//                    for (int i = 0; i < map.GetLength(0); i++)
//                    {
//                        if (map[i, j] == null && map[i, j + 1] != null)
//                        {
//                            SwapCells(new Point(i, j), new Point(i, j + 1), gameEventsListener, currentStepIndex, ref map);
//                            swaps++;
//                        }
//                    }
//                }
//            }
//            while (swaps != 0);
//            return result;
//        }
//        void CreateNewItems()
//        {
//            for (int i = 0; i < map.GetLength(0); i++)
//            {
//                for (int j = map.GetLength(1) - 1; j > -1; j--)
//                {
//                    if (map[i, j] == null)
//                    {
//                        map[i, j] = new Match3MapItem((int)Math.Round((cellTypesCount - 1) * rand.NextDouble()));
//                        gameEventsListener.OnCellCreate(i, j, map[i, j].ItemType, currentStepIndex);
//                    }
//                }
//            }
//        }

//        static Point[] GetVerticalHint(Match3MapItem[,] actualMap)
//        {
//            var submap = TransposeMap(actualMap);
//            var result = GetHorizontalHint(submap);
//            return GetTransposedPoints(result);
//        }
//        static Point[] GetHorizontalHint(Match3MapItem[,] actualMap)
//        {
//            for (int i = 0; i < checkingStep; i++)
//            {
//                for (int j = 0; j < checkingStep; j++)
//                {
//                    var mapCopy = CopyMap(actualMap);

//                    for (int x = i; x < mapCopy.GetLength(0) - 1; x += checkingStep)
//                    {
//                        for (int y = j; y < mapCopy.GetLength(1); y += checkingStep)
//                        {
//                            SwapCells(new Point(x, y), new Point(x + 1, y), null, -1, ref mapCopy);
//                        }
//                    }

//                    List<Point[]> bangLines = new List<Point[]>();
//                    bangLines.AddRange(GetAllHorizontalBangLines(mapCopy));
//                    bangLines.AddRange(GetAllVerticalBangLines(mapCopy));
//                    if (bangLines.Count > 0)
//                    {
//                        mapCopy = CopyMap(actualMap);
//                        for (int x = i; x < mapCopy.GetLength(0) - 1; x += checkingStep)
//                        {
//                            for (int y = j; y < mapCopy.GetLength(1); y += checkingStep)
//                            {
//                                SwapCells(new Point(x, y), new Point(x + 1, y), null, -1, ref mapCopy);
//                                bangLines = new List<Point[]>();
//                                bangLines.AddRange(GetAllHorizontalBangLines(mapCopy));
//                                bangLines.AddRange(GetAllVerticalBangLines(mapCopy));
//                                if (bangLines.Count > 0)
//                                {
//                                    return new Point[] { new Point(x, y), new Point(x + 1, y) };
//                                }
//                                else
//                                {
//                                    SwapCells(new Point(x, y), new Point(x + 1, y), null, -1, ref mapCopy);
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            return null;
//        }

//        static List<Point[]> GetAllVerticalBangLines(Match3MapItem[,] actualMap)
//        {
//            var submap = TransposeMap(actualMap);
//            var result = GetAllHorizontalBangLines(submap);
//            return result.Select(bangLine => GetTransposedPoints(bangLine)).ToList();
//        }
//        static List<Point[]> GetAllHorizontalBangLines(Match3MapItem[,] actualMap)
//        {
//            var result = new List<Point[]>();
//            for (int i = 0; i < actualMap.GetLength(0) - 2; i++)
//            {
//                for (int j = 0; j < actualMap.GetLength(1); j++)
//                {
//                    if (actualMap[i, j] == null || actualMap[i, j].IsBlocked)
//                    {
//                        continue;
//                    }
//                    var startPointType = actualMap[i, j].ItemType;
//                    int lineLength = 0;
//                    while (actualMap.GetLength(0) > i + lineLength && actualMap[i + lineLength, j].ItemType == startPointType)
//                    {
//                        lineLength++;
//                    }
//                    if (lineLength > 2)
//                    {
//                        var bangLine = new Point[lineLength];
//                        for (int k = 0; k < lineLength; k++)
//                        {
//                            bangLine[k] = new Point(i + k, j);
//                        }
//                        result.Add(bangLine);
//                    }
//                }
//            }
//            return result;

//        }

//        static void SwapCells(Point cell0, Point cell1, IGameEventsListener gameEventsListener, int stepIndex, ref Match3MapItem[,] targetMap)
//        {
//            var tempCell = targetMap[cell0.x, cell0.y];
//            targetMap[cell0.x, cell0.y] = targetMap[cell1.x, cell1.y];
//            targetMap[cell1.x, cell1.y] = tempCell;
//            if (gameEventsListener != null)
//            {
//                gameEventsListener.OnCellMove(cell0.x, cell0.y, cell1.x, cell1.y, stepIndex);
//            }
//        }
//        static Match3MapItem[,] CopyMap(Match3MapItem[,] targetMap)
//        {
//            var result = new Match3MapItem[targetMap.GetLength(0), targetMap.GetLength(1)];
//            for (int i = 0; i < targetMap.GetLength(0); i++)
//            {
//                for (int j = 0; j < targetMap.GetLength(1); j++)
//                {
//                    result[i, j] = targetMap[i, j];
//                }
//            }
//            return result;
//        }
//        static Match3MapItem[,] TransposeMap(Match3MapItem[,] targetMap)
//        {
//            var result = new Match3MapItem[targetMap.GetLength(1), targetMap.GetLength(0)];
//            for (int i = 0; i < targetMap.GetLength(0); i++)
//            {
//                for (int j = 0; j < targetMap.GetLength(1); j++)
//                {
//                    result[j, i] = targetMap[i, j];
//                }
//            }
//            return result;
//        }
//        static Point[] GetTransposedPoints(Point[] points)
//        {
//            return points == null ? null : points.Select(item => new Point(item.y, item.x)).ToArray();
//        }
//        static bool IsPointValid(Point point, int width, int height)
//        {
//            return point.x > -1 && point.y > -1 && point.x < width && point.y < height;
//        }
      
//    }
//}