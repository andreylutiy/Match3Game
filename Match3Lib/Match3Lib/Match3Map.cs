using System;
using System.Collections.Generic;

namespace Match3Lib
{
    internal class Match3Map
    {
        public int Width { get { return map.GetLength(0); } }
        public int Height { get { return map.GetLength(1); } }

        public Point Size { get { return new Point(Width, Height); } }

        Match3MapItem[,] map;
        IMapEventsListener mapEventsListener;

        public Match3Map(int width, int height, IMapEventsListener currentMapEventsListener = null)
        {
            this.map = new Match3MapItem[width, height];
            this.mapEventsListener = currentMapEventsListener;
        }

        public Match3Map(Match3Map templateMap)
        {
            this.map = new Match3MapItem[templateMap.Width, templateMap.Height];

            for (int i = 0; i < this.Width; i++)
                for (int j = 0; j < this.Height; j++)
                    if (templateMap.map[i, j] != null)
                        this.map[i, j] = new Match3MapItem(templateMap.map[i, j]);
        }

        public void CreateCell(Point cell, int cellType)
        {
            if (!Utils.IsCellInRange(cell, Size))
                throw new ArgumentOutOfRangeException("Match3Lib.Match3Map.CreateCells exception: cell out of range!");

            map[cell.x, cell.y] = new Match3MapItem(cellType);

            if (mapEventsListener != null)
                mapEventsListener.OnCellCreate(cell, cellType);
        }

        public void SwapCells(Point cell0, Point cell1)
        {
            var isCellsValid = true;
            isCellsValid = isCellsValid && Utils.IsCellInRange(cell0, Size);
            isCellsValid = isCellsValid && Utils.IsCellInRange(cell1, Size);

            if (!isCellsValid)
                throw new ArgumentOutOfRangeException("Match3Lib.Match3Map.SwapCells exception: cell(s) out of range!");

            var tempCell = map[cell0.x, cell0.y];
            map[cell0.x, cell0.y] = map[cell1.x, cell1.y];
            map[cell1.x, cell1.y] = tempCell;

            if (mapEventsListener != null)
                mapEventsListener.OnCellMove(cell0, cell1);
        }

        public void DestroyCell(Point cell)
        {
            if (!Utils.IsCellInRange(cell, Size))
                throw new ArgumentOutOfRangeException("Match3Lib.Match3Map.DestroyCells exception: cell out of range!");

            if (map[cell.x, cell.y] == null)
                return;

            map[cell.x, cell.y] = null;

            if (mapEventsListener != null)
                mapEventsListener.OnCellDestroy(cell);
        }

        public List<Point> GetCellIndexesByType(int cellType)
        {
            var result = new List<Point>();

            for (int i = 0; i < this.Width; i++)
                for (int j = 0; j < this.Height; j++)
                    if (map[i, j] != null && map[i, j].ItemType == cellType)
                        result.Add(new Point(i, j));

            return result;
        }

        public Match3Map TransposeMap()
        {
            var result = new Match3Map(this.Height, this.Width);

            for (int i = 0; i < this.Width; i++)
                for (int j = 0; j < this.Height; j++)
                    if (this.map[i, j] != null)
                        result.map[j, i] = new Match3MapItem(this.map[i, j]);

            return result;
        }

        public Match3MapItem this[int x, int y]
        {
            get
            {
                return this[new Point(x, y)];
            }
        }

        public Match3MapItem this[Point key]
        {
            get
            {
                if (!Utils.IsCellInRange(key, Size))
                    throw new ArgumentOutOfRangeException("Match3Lib.Match3Map exception: map item (" + key.x.ToString() + "; " + key.y.ToString() + ")is out of range!");

                return map[key.x, key.y];
            }
        }
    }
}