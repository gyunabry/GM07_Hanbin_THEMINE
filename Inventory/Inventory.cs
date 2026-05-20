namespace TheMine
{
    public class Inventory
    {
        // Key : 슬롯 인덱스, Value : 인벤토리 슬롯 데이터
        public Dictionary<int, InventorySlot> Slots { get; private set; } = new Dictionary<int, InventorySlot>();
        public int MaxSlots { get; private set; } = 10;

        public Inventory()
        {
            for (int i = 1; i <= MaxSlots; i++)
            {
                Slots.Add(i, new InventorySlot());
            }
        }

        // 아이템 추가
        // 같은 아이템이 있는지 찾고 있다면 count++
        public void AddItem(Item newItem, int count = 1)
        {
            // 이미 있는 소비 아이템이라면 수량만 증가
            if (newItem is IConsumable)
            {
                foreach (var slot in Slots.Values)
                {
                    // 해당 슬롯이 비어있지 않고
                    // 추가하려는 아이템이 해당 슬롯에 있는 아이템 이름과 같은지 체크
                    if (!slot.IsEmpty && slot.ItemData.Name == newItem.Name)
                    {
                        slot.Count += count;
                        return;
                    }
                }
            }

            // 장비나 새 아이템이라면 빈 슬롯 찾아서 추가
            foreach (var slot in Slots.Values)
            {
                if (slot.IsEmpty)
                {
                    slot.ItemData = newItem;
                    slot.Count = count;
                    return;
                }
            }
        }

        // 인덱스를 전달받아 해당 인덱스에 해당하는 아이템 제거
        // 아이템 사용 및 판매 시 사용
        public void RemoveItem(int slotKey, int amount = 1)
        {
            if (Slots.TryGetValue(slotKey, out InventorySlot? slot) && !slot.IsEmpty)
            {
                slot.Count -= amount;

                if (slot.Count <= 0)
                {
                    slot.ItemData = null;
                    slot.Count = 0;
                }
            }
        }
    }
}
