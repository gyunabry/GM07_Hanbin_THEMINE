namespace TheMine
{
    enum Map 
    { 
        Town,
        Shop,
        Dungeon,
        Inventory
    }

    enum Shop
    {
        Menu,
        Buy,
        Sell
    }

    // 맵 이동을 관장하는 매니저
    class MapManager
    {
        public Map CurrentMap { get; private set; } = Map.Town;
        public Shop CurrentShop { get; private set; } = Shop.Menu;

        public void MoveToOtherMap(Map targetMap)
        {
            if (CurrentMap != targetMap)
            {
                CurrentMap = targetMap;
            }
        }

        public void MoveToShopMenu(Shop target)
        {
            if (CurrentShop != target)
            {
                CurrentShop = target;
            }
        }

        public void ReturnToTown()
        {
            CurrentMap = Map.Town;
        }
    }
}
