namespace TheMine
{
    interface IConsumable
    {
        // HP, MP 회복, 공격력 증가 등에 쓰일 수치
        public int Value { get; set; }

        public void UseItem();
    }
}
