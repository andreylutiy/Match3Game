using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Match3Lib
{
    internal class Match3Map
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Point Size { get { return new Point(Width, Height); } }

        Match3MapItem[,] map;

        public Match3Map(int width, int height)
        {
            this.map = new Match3MapItem[width, height];
            this.Width = width;
            this.Height = height;
        }
        public Match3Map(Match3Map templateMap)
        {
            this.map = new Match3MapItem[templateMap.Width, templateMap.Height];
            this.Width = templateMap.Width;
            this.Height = templateMap.Height;
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    this.map[i, j] = new Match3MapItem(templateMap.map[i,j]);
                }
            }
        }

        public Match3Map TransposeMap()
        {
            var result = new Match3Map(this.Height, this.Width);
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    result.map[i, j] = new Match3MapItem(this.map[j, i]);
                }
            }
            return result;
        }

        public void SwapCells(Point cell0, Point cell1)
        {
            var isCellsValid = true;
            isCellsValid =isCellsValid && Utils.IsCellInRange(cell0, Size);
            isCellsValid =isCellsValid && Utils.IsCellInRange(cell1, Size);
            if (!isCellsValid)
            {
                throw new ArgumentOutOfRangeException("Match3Lib.Match3Map.SwapCells exception: cell(s) out of range!");
            }
            var tempCell = map[cell0.x, cell0.y];
            map[cell0.x, cell0.y] = map[cell1.x, cell1.y];
            map[cell1.x, cell1.y] = tempCell;
        }
    }
}
