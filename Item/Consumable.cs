namespace TheMine
{
    class Consumable : Item, IConsumable
    {
        public int Value { get; set; }

        public Consumable() { }

        public static Consumable CreateTestPotion()
        {
            return new Consumable
            {
                Id = "test_potion",
                Name = "테스트 포션",
                Price = 100,
                SellPrice = 50,
                Description = "UI 테스트용 회복 아이템입니다.",
                Value = 30
            };
        }

        public void UseItem()
        {
            // 아이템 사용 메서드
        }
    }
}
