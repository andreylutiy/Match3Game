namespace Match3Lib
{
    internal class Match3MapItem
    {
        public int ItemType { get; private set; }
        public bool IsBlocked { get; private set; }

        public Match3MapItem(Match3MapItem template)
        {
            ItemType = template.ItemType;
            IsBlocked = template.IsBlocked;
        }

        public Match3MapItem(int currentItemType, bool needBlock = false)
        {
            ItemType = currentItemType;
            IsBlocked = needBlock;
        }

        public void Block()
        {
            IsBlocked = true;
        }

        public void Unblock()
        {
            IsBlocked = false;
        }
    }
}
