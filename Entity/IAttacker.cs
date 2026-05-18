namespace TheMine
{
    // 공격가능한 엔티티들이 구현할 인터페이스
    interface IAttackable

    {
        public int AttackPower { get; set; }

        public void Attack(Entity target);
    }
}
