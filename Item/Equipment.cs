namespace TheMine
{
    public abstract class Equipment : Item
    {
        public int Durability { get; set; }
        public int UpgradeValue { get; set; }
        public bool IsEquipped { get; set; }
    }

    public class Pickaxe : Equipment 
    { 
        public int BaseMiningPower { get; set; }
        public int CritChance { get; set; }

        public Pickaxe() { }

        public Pickaxe(string id, string name, int durability, int miningPower, int upgradeValue, int critChance, int price, int sellPrice, string description )
        {
            // Item 속성
            Id = id;
            Name = name;
            Price = price;
            SellPrice = sellPrice;

            // Equipment 속성
            Durability = durability;
            UpgradeValue = upgradeValue;
            Description = description;
            IsEquipped = false;

            BaseMiningPower = miningPower;
            CritChance = critChance;
        }
    }

    public class Armor : Equipment
    {
        public int BaseDefense { get; set; }

        public Armor() { }

        public Armor(string id, string name, int durability, int def, int upgradeValue, int price, int sellPrice, string description)
        {
            // Item 속성
            Id = id;
            Name = name;
            Price = price;
            SellPrice = sellPrice;

            // Equipment 속성
            Durability = durability;
            UpgradeValue = upgradeValue;
            Description = description;
            IsEquipped = false;

            BaseDefense = def;
        }
    }
}
