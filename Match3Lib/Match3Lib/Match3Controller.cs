using System;
using System.Collections.Generic;

namespace Match3Lib
{
    public class Match3Controller : IMapEventsListener
    {
        public int ActualScore
        {
            get
            {
                return _actualScore;
            }
            private set
            {
                _actualScore = value;

                if (gameEventsListener != null)
                    gameEventsListener.OnScore(_actualScore, currentStepIndex);
            }
        }

        private int _actualScore;
        private IGameEventsListener gameEventsListener;
        private int cellTypesCount;
        private Match3Map map;
        private int currentStepIndex;

        public void NewGame(Point size, IGameEventsListener actualGameEventsListener, int actualCellTypesCount = 3)
        {
            if (size == null)
                throw new NullReferenceException("Match3Lib.Match3Controller.NewGame exception: 'size' can't be null!");

            if (!Utils.IsCellInRange(size - Constants.minSize, Constants.maxSize - Constants.minSize))
                throw new ArgumentOutOfRangeException("Match3Lib.Match3Controller.NewGame exception: map size out of range!");

            if (actualCellTypesCount < 2)
                throw new ArgumentOutOfRangeException("Match3Lib.Match3Controller.NewGame exception: cellTypesCount out of range!");

            map = new Match3Map(size.x, size.y, this);
            cellTypesCount = actualCellTypesCount;
            currentStepIndex = 0;
            gameEventsListener = actualGameEventsListener;
            ActualScore = 0;
            UpdateMapState();
        }

        public bool MakeStep(Point cell0, Point cell1)
        {
            map.SwapCells(cell0, cell1);

            if (
                (cell0 - cell1).Magnitude() > 1
             || UpdateMapState() == 0
             )
            {
                currentStepIndex++;
                map.SwapCells(cell0, cell1);

                return false;
            }
            else
            {
                return true;
            }
        }

        private int UpdateMapState()
        {
            var result = 0;
            var isCurrentMapValid = true;

            do
            {
                if (!isCurrentMapValid)
                    Match3MapManager.ResetMap(map, cellTypesCount);

                var previosResult = 0;

                do
                {
                    previosResult = result;
                    var bangLines = Match3BangController.GetAllBangLines(map);

                    if (bangLines.Count == 0)
                        continue;

                    var comboBangPoints = Match3BangController.GetComboBangItems(map, bangLines);
                    var currentStepScore = ScoreManager.GetScore(bangLines, comboBangPoints);
                    var cellsForDestroy = new List<Point>();
                    currentStepIndex++;
                    Match3MapManager.DestroyStepCells(map, bangLines, comboBangPoints);
                    result += currentStepScore;
                    ActualScore += currentStepScore;
                    Match3MapManager.MoveDownAllPossibleCells(map);
                    Match3MapManager.FillAllEmptyCells(map, cellTypesCount);
                }
                while (previosResult != result);

                isCurrentMapValid = MapStateManager.IsMapValid(map);
            }
            while (!isCurrentMapValid);

            return result;
        }

        void IMapEventsListener.OnCellCreate(Point cellPos, int cellType)
        {
            gameEventsListener?.OnCellCreate(cellPos, cellType, currentStepIndex);
        }

        void IMapEventsListener.OnCellMove(Point oldPos, Point newPos)
        {
            gameEventsListener?.OnCellMove(oldPos, newPos, currentStepIndex);
        }

        void IMapEventsListener.OnCellDestroy(Point cellPos)
        {
            gameEventsListener?.OnCellDestroy(cellPos, currentStepIndex);
        }
    }
}