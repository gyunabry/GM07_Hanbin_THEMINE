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
            ShopItems.Add(new Consumable {
                Id = "C001",
                Name = "빨간 물약",
                Price = 50,
                SellPrice = 20,
                Description = "사용 시 HP를 30 회복합니다.",
                OnUseEffect = (player) =>
                {
                    if (!player.CanApplyEffect(EffectType.HP))
                    {
                        ConsoleRenderer.AddLog("이미 체력이 최대치라 사용할 수 없습니다.");
                        return false;
                    }
                    player.RestoreHp(30);
                    ConsoleRenderer.AddLog("빨간 물약을 사용하여 체력을 30 회복했습니다.");
                    return true;
                }
            });

            ShopItems.Add(new Consumable { 
                Id = "C002", 
                Name = "파란 물약", 
                Price = 50, 
                SellPrice = 20, 
                Description = "사용 시 MP를 20 회복합니다.",
                OnUseEffect = (player) =>
                {
                    if (!player.CanApplyEffect(EffectType.MP))
                    {
                        ConsoleRenderer.AddLog("이미 마나가 최대치라 사용할 수 없습니다.");
                        return false;
                    }
                    player.RestoreMp(20);
                    ConsoleRenderer.AddLog("파란 물약을 사용하여 체력을 30 회복했습니다.");
                    return true;
                }
            });
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

        public void SellItem(int slotKey, Player player)
        {
            if (!player.Inventory.Slots.ContainsKey(slotKey) && player.Inventory.Slots[slotKey].IsEmpty)
            {
                ConsoleRenderer.AddLog("잘못된 슬롯입니다.");
                return;
            }

            var slotItem = player.Inventory.Slots[slotKey];
            Item targetItem = slotItem.ItemData;

            // 판매하려는 아이템이 만약 장비이고 장착 중이라면 해제
            if (targetItem is Equipment eq && eq.IsEquipped)
            {
                player.ToggleEquip(eq);
            }

            ConsoleRenderer.AddLog($"{targetItem.Name}을(를) 판매해 {targetItem.SellPrice} Gold를 획득했습니다.");
            player.GainGold(targetItem.SellPrice);
            player.Inventory.RemoveItem(slotKey, 1);
            
        }

        // 상점의 아이템 객체를 반환하는 메서드
        private Item CloneItem(Item sourceItem)
        {
            if (sourceItem is Consumable c)
                return new Consumable { Id = c.Id, Name = c.Name, Price = c.Price, SellPrice = c.SellPrice, Description = c.Description, OnUseEffect = c.OnUseEffect };
            if (sourceItem is Pickaxe p)
                return new Pickaxe(p.Id, p.Name, p.Durability, p.BaseMiningPower, p.UpgradeValue, p.CritChance, p.Price, p.SellPrice, p.Description);
            if (sourceItem is Armor a)
                return new Armor(a.Id, a.Name, a.Durability, a.BaseDefense, a.UpgradeValue, a.Price, a.SellPrice, a.Description);

            return sourceItem;
        }
    }
}
