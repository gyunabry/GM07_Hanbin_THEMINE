namespace TheMine
{
    class Monster : Entity, IAttackable
    {
        public int DropGold { get; set; }
        public int DropExp { get; set; }
        public int AttackPower { get; set; }
        public string Description { get; set; }

        public Monster() { }

        public Monster(string id, string name, int maxHp, int attackPower, int def, int dropGold, int dropExp, string description)
        {
            Id = id;
            Name = name;
            MaxHp = maxHp;
            Hp = maxHp;
            AttackPower = attackPower;
            Def = def;
            DropGold = dropGold;
            DropExp = dropExp;
            Description = description;
            IsDead = false;
        }

        public void Attack(Entity target)
        {
            target.TakeDamage(AttackPower);
        }

        public override void TakeDamage(int amount)
        {
            int finalDamage = Math.Max(0, amount - Def);
            ApplyDamage(finalDamage);
        }
    }
}
