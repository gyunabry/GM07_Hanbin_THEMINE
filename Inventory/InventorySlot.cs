namespace TheMine
{
    class InventorySlot
    {
        public Item ItemData { get; set; }
        public int Count { get; set; }

        // 해당 슬롯이 비어있는지
        public bool IsEmpty => ItemData == null;

        public void Clear()
        {
            ItemData = null;
            Count = 0;
        }
    }
}