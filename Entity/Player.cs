namespace TheMine
{
    public class Player : Entity, IAttackable
    {
        public int Mp { get; private set; }
        public int MaxMp { get; private set; }
        public int Level { get; private set; }
        public int Exp { get; private set; }
        public int Gold { get; private set; } = 1000;
        public int AttackPower { get; set; }

        public Pickaxe EquippedPickaxe { get; private set; }
        public Armor EquippedArmor { get; private set; }

        // 장비 능력치를 포함한 최종 공격력/방어력
        public int TotalAttackPower => AttackPower + (EquippedPickaxe?.BaseMiningPower ?? 0);
        public int ToalDefense => Def + (EquippedArmor?.BaseDefense ?? 0);
        public int TotalDefense => ToalDefense;

        public Inventory Inventory { get; set; }

        // 버프/디버프 중복 적용을 막기 위한 목록
        public HashSet<StatusEffect> ActiveEffects { get; private set; }

        public Player(string name, int maxHp, int maxMp, int attackPower, int def)
        {
            Name = name;
            MaxHp = maxHp;
            Hp = maxHp;
            MaxMp = maxMp;
            Mp = maxMp;
            AttackPower = attackPower;
            Def = def;
            Level = 1;
            Exp = 0;
            IsDead = false;
            ActiveEffects = new HashSet<StatusEffect>();
        }

        public void Attack(Entity target)
        {
            target.TakeDamage(TotalAttackPower);
        }

        public override void TakeDamage(int amount)
        {
            int finalDamage = Math.Max(0, amount - TotalDefense);
            ApplyDamage(finalDamage);
        }

        /// <summary>
        /// 경험치 획득
        /// </summary>
        public void GainExp(int amount)
        {
            Exp += amount;

            while (Exp >= GetRequiredExpForNextLevel())
            {
                Exp -= GetRequiredExpForNextLevel();
                LevelUp();
            }
        }

        public void GainGold(int amount)
        {
            Gold += amount;
        }

        public void UseGold(int amount)
        {
            Gold -= amount;
        }

        private void LevelUp()
        {
            Level++;
            // TODO: 레벨업 시 스탯 증가 또는 스탯 분배 구현
        }

        public int GetRequiredExpForNextLevel()
        {
            // TODO: 레벨별 필요 경험치 계산식 구체화
            return Level * 100;
        }

        public void RestorePlayerState(Player player)
        {
            player.Hp = MaxHp;
            player.Mp = MaxMp;
            player.IsDead = false;
        }

        public void RestoreHp(int amount)
        {
            ConsoleRenderer.AddLog($"HP : {amount} 회복");
            Hp += amount;
            if (Hp > MaxHp)
            {
                Hp = MaxHp;
            }
        }

        public void RestoreMp(int amount)
        {
            ConsoleRenderer.AddLog($"MP : {amount} 회복");
            Mp += amount;
            if (Mp > MaxMp)
            {
                Mp = MaxMp;
            }
        }

        public bool CanApplyEffect(EffectType type)
        {
            switch (type)
            {
                case EffectType.HP:
                    return Hp < MaxHp;

                case EffectType.MP:
                    return Mp < MaxMp;

                case EffectType.AttackBuff:
                    // TODO: 나중에 버프 시스템이 추가되면 이미 공격력 버프가 있는지 검사
                    // return !HasAttackBuff; 
                    return true; // 지금은 무조건 사용 가능하도록 true 반환

                case EffectType.DefenseBuff:
                    // TODO: 방어력 버프 검사 로직
                    return true;

                default:
                    return true;
            }
        }

        public void ToggleEquip(Equipment equip)
        {
            // 장착 중이라면 장비 해제
            if (equip.IsEquipped)
            {
                equip.IsEquipped = false;
                if (equip is Pickaxe) EquippedPickaxe = null;
                if (equip is Pickaxe) EquippedPickaxe = null;
                ConsoleRenderer.AddLog($"{equip.Name}을(를) 장착 해제했습니다.");
            }
            // 장착 중이 아니라면 새로 장착
            // 이미 장착 중인 다른 장비가 있다면 먼저 해제 후 장착
            else
            {
                if (equip is Pickaxe pickaxe)
                {
                    if (EquippedPickaxe != null) 
                        EquippedPickaxe.IsEquipped = false;
                    EquippedPickaxe = pickaxe;
                }
                else if (equip is Armor armor)
                {
                    if (EquippedArmor != null)
                        EquippedArmor.IsEquipped = false;
                    EquippedArmor = armor;
                }

                equip.IsEquipped = true;
                ConsoleRenderer.AddLog($"{equip.Name}을(를) 장착했습니다.");
            }
        }
    }
}
