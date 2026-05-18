namespace TheMine
{
    class ShopManager
    {
        public List<Item> ShopItems { get; private set; } = new List<Item>();
        DataManager dm = DataManager.Instance;

        public void InitShop()
        {
            ShopItems.Clear();

            // 소비 아이템 추가
            ShopItems.Add(new Consumable { Id = "C001", Name = "빨간 물약", Price = 50, SellPrice = 20, Description = "사용 시 HP를 30 회복합니다." });
            ShopItems.Add(new Consumable { Id = "C002", Name = "파란 물약", Price = 50, SellPrice = 20, Description = "사용 시 MP를 20 회복합니다." });
            // ShopItems.Add(new Consumable { Id = "C003", Name = "분노 물약", Price = 100, SellPrice = 40, Description = "사용 시 공격력이 10 증가합니다." });

            // 곡괭이 및 방어구 추가
            foreach (var pickaxe in dm.pickaxeDict.Values)
            {
                ShopItems.Add(new Pickaxe(
                    pickaxe.Id, 
                    pickaxe.Name, 
                    pickaxe.Durability,
                    pickaxe.BaseMiningPower, 
                    pickaxe.UpgradeValue, 
                    pickaxe.CritChance,
                    pickaxe.Price, 
                    pickaxe.SellPrice, 
                    pickaxe.Description)
                );
            }

            foreach (var armor in dm.armorDict.Values)
            {
                ShopItems.Add(new Armor(
                    armor.Id,
                    armor.Name,
                    armor.Durability,
                    armor.BaseDefense,
                    armor.UpgradeValue,
                    armor.Price,
                    armor.SellPrice,
                    armor.Description)
                );
            }
        }

        public void BuyItem(int index, Player player)
        {
            if (index < 0 || index >= ShopItems.Count)
            {
                return;
            }

            Item targetItem = ShopItems[index];

            // 구매 조건 확인 (소지금 검사)
            if (player.Gold < targetItem.Price)
            {
                ConsoleRenderer.AddLog("골드가 부족합니다.");
                return;
            }

            // 플레이어 골드 차감
            player.UseGold(targetItem.Price);

            // 인벤토리에 아이템 추가
            Item item = CloneItem(targetItem);
            player.Inventory.AddItem(item);

            ConsoleRenderer.AddLog($"{targetItem.Name}을(를) 구매했습니다.");
        }

        // 상점의 아이템 객체를 반환하는 메서드
        private Item CloneItem(Item sourceItem)
        {
            if (sourceItem is Consumable c)
                return new Consumable { Id = c.Id, Name = c.Name, Price = c.Price, SellPrice = c.SellPrice, Description = c.Description, Value = c.Value };
            if (sourceItem is Pickaxe p)
                return new Pickaxe(p.Id, p.Name, p.Durability, p.BaseMiningPower, p.UpgradeValue, p.CritChance, p.Price, p.SellPrice, p.Description);
            if (sourceItem is Armor a)
                return new Armor(a.Id, a.Name, a.Durability, a.BaseDefense, a.UpgradeValue, a.Price, a.SellPrice, a.Description);

            return sourceItem;
        }
    }
}
