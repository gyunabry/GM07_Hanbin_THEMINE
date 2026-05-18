namespace TheMine
{
    enum Map 
    { 
        Town,
        Shop,
        Dungeon,
        Equipment
    }

    // 맵 이동을 관장하는 매니저
    class MapManager
    {
        public Map CurrentMap { get; private set; } = Map.Town;

        public void MoveToOtherMap(Map targetMap)
        {
            if (CurrentMap != targetMap)
            {
                CurrentMap = targetMap;
            }
        }

        public void ReturnToTown()
        {
            CurrentMap = Map.Town;
        }
    }
}
