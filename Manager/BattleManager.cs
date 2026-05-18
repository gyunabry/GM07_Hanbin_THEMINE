namespace TheMine
{
    class BattleManager
    {
        private Player _player;
        private Monster _monster;
        private bool _isEscaped;

        public void Init(Player player, Monster monster)
        {
            _player = player;
            _monster = monster;
            _isEscaped = false;
        }

        // true : 계속 진행 / false : 귀환/사망
        public bool StartBattle()
        {
            ConsoleRenderer.AddLog($"{_monster.Name}을(를) 조우했습니다.");
            ConsoleRenderer.AddLog(_monster.Description);

            while (!_player.IsDead && !_monster.IsDead && !_isEscaped)
            {
                ConsoleRenderer.RenderBattleScreen(_player, _monster);
                NextTurn();
            }

            ConsoleRenderer.RenderBattleScreen(_player, _monster);

            if (_monster.IsDead)
            {
                GiveRewards();

                if (GameManager.Instance.CurrentDepth >= 50)
                {
                    ConsoleRenderer.AddLog("보스 몬스터를 처치했습니다! 던전 탐험 완료!");
                    ConsoleRenderer.AddLog($"아무키나 눌러 마을로 복귀");
                    return false;
                }

                ConsoleRenderer.RenderAfterBattleScreen(_player, _monster);
                int select = ConsoleRenderer.ReadAfterBattleChoice();

                if (select == 1) // 던전 탐험 진행
                {
                    Random rand = new Random();

                    // 1~20층까지 랜덤 이동
                    int dropDepth = rand.Next(45, 50); // TODO: 배포 시 해당 값 복구, 디버그용으로 현재 값 사용
                    GameManager.Instance.CurrentDepth += dropDepth;
                    ConsoleRenderer.AddLog($"더 깊은 곳으로 내려갑니다...");
                    return true;
                }
                else // 마을로 귀환
                {
                    ConsoleRenderer.AddLog($"마을로 귀환합니다.");
                    ConsoleRenderer.AddLog($"아무키나 눌러 마을로 복귀");
                    return false;
                }
            }
            else if (_player.IsDead)
            {
                ConsoleRenderer.AddLog($"아무키나 눌러 마을로 복귀");
                return false;
            }
            else if (_isEscaped)
            {
                ConsoleRenderer.AddLog("전투에서 도망쳤습니다.");
                ConsoleRenderer.AddLog($"아무키나 눌러 마을로 복귀");
                return false;
            }

            return false;
        }

        public void NextTurn()
        {
            int choice = ConsoleRenderer.ReadBattleChoice();

            switch (choice)
            {
                case 1:
                    PlayerAttack();
                    if (!_monster.IsDead)
                    {
                        MonsterAttack();
                    }
                    break;
                case 2:
                    ConsoleRenderer.AddLog("아직 쓸 수 있는 스킬이 없다.");
                    break;
                case 3:
                    ConsoleRenderer.AddLog("이정도 적에게 아이템 사용은 수치다.");
                    break;
                case 4:
                    _isEscaped = true;
                    break;
            }
        }

        public void ProcessStatusEffects()
        {
            // TODO: 상태 이상 처리 구현
        }

        public void GiveRewards()
        {
            _player.GainGold(_monster.DropGold);
            _player.GainExp(_monster.DropExp);
            ConsoleRenderer.AddLog($"{_monster.Name}을(를) 쓰러뜨렸습니다.");
            ConsoleRenderer.AddLog($"{_monster.DropGold} Gold와 {_monster.DropExp} Exp를 획득했습니다.");
        }

        private void PlayerAttack()
        {
            _player.Attack(_monster);
            ConsoleRenderer.AddLog($"{_player.Name}이(가) {_monster.Name}에게 {_monster.LastDamageTaken}의 피해를 입힘");

            if (_monster.IsDead)
            {
                ConsoleRenderer.AddLog($"{_monster.Name}이(가) 쓰러졌습니다.");
            }
        }

        private void MonsterAttack()
        {
            _monster.Attack(_player);
            ConsoleRenderer.AddLog($"{_monster.Name}이(가) {_player.Name}에게 {_player.LastDamageTaken}의 피해를 입힘");

            if (_player.IsDead)
            {
                ConsoleRenderer.AddLog($"{_player.Name}이(가) 쓰러졌습니다.");
            }
        }
    }
}
