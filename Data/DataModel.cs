using System;

namespace TheMine
{
    // 게임에 필요한 데이터들을 만들 클래스를 정리

    public class MonsterData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxHp { get; set; }
        public int AttackPower { get; set; }
        public int Def { get; set; }
        public int DropGold { get; set; }
        public int DropExp { get; set; }
        public string Description { get; set; }
    }

    enum EquipmentType
    {
        None = 0,
        Pickaxe,
        Armor
    }

    public class EquipmentData
    {
        // public EquipmentType Type { get; set; }
    }

}
