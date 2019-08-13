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

    internal interface IMapEventsListener
    {
        void OnCellCreate(Point cellPos, int cellType);
        void OnCellMove(Point oldPos, Point newPos);
        void OnCellDestroy(Point cellPos);
    }
}
