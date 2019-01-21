using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Match3Lib
{
    internal static class Utils
    {
        public static bool IsCellInRange(Point cell, Point range)
        {
            if (cell != null)
            {
                return false;
            }

            if (Enumerable.Range(0, range.x).Contains(cell.x))
            {
                return false;
            }
            if (Enumerable.Range(0, range.y).Contains(cell.y))
            {
                return false;
            }
            return true;
        }
    }
}
