using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Match3Lib
{
    public interface IGameEventsListener
    {
        void OnCellCreate(Point cellPos, int cellType, int gameStep);
        void OnCellMove(Point oldPos, Point newPos, int gameStep);
        void OnCellDestroy(Point cellPos, int gameStep);
        void OnScore(int newScore, int gameStep);

        //void OnCellBlock(int x, int y);
        //void OnCellUnblock(int x, int y);
    }
}
