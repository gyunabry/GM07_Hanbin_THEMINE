namespace TheMine
{
    // 모든 전투 대상이 공통으로 가지는 상태
    abstract class Entity
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public int Hp { get; protected set; }
        public int MaxHp { get; protected set; }
        public int Def { get; protected set; }
        public bool IsDead { get; protected set; } = false;
        public int LastDamageTaken { get; protected set; }

        public abstract void TakeDamage(int amount);

        protected void ApplyDamage(int amount)
        {
            LastDamageTaken = Math.Max(0, amount);
            Hp -= LastDamageTaken;

            if (Hp <= 0)
            {
                Hp = 0;
                IsDead = true;
            }
        }
    }
}
