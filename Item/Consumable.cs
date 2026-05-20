namespace TheMine
{
    public class Consumable : Item, IConsumable
    {
        public Func<Player, bool> OnUseEffect { get; set; }

        public EffectType effectType { get; set; }

        public int Value { get; set; }

        public bool UseItem(Player player)
        {
            // 아이템 사용 메서드
            if (OnUseEffect != null)
            {
                return OnUseEffect.Invoke(player);
            }
            return false;
        }
    }
}
