namespace TheMine
{
    class BattleManager
    {
        private Player _player;
        private Monster _monster;
        private bool _isEnd;
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
            _isEnd = false;

            ConsoleRenderer.AddLog($"{_monster.Name}을(를) 조우했습니다.");
            ConsoleRenderer.AddLog(_monster.Description);

            while (!_player.IsDead && !_monster.IsDead && !_isEscaped)
            {
                ConsoleRenderer.RenderBattleScreen(_player, _monster, _isEnd);
                NextTurn();
            }

            ConsoleRenderer.RenderBattleScreen(_player, _monster, _isEnd);

            if (_monster.IsDead)
            {
                _isEnd = true;

                GiveRewards();

                if (GameManager.Instance.CurrentDepth >= 50)
                {
                    ConsoleRenderer.AddLog("보스 몬스터를 처치했습니다! 던전 탐험 완료!");
                    ConsoleRenderer.AddLog($"아무키나 눌러 마을로 복귀");
                    return false;
                }

                // ConsoleRenderer.RenderAfterBattleScreen(_player, _monster);
                ConsoleRenderer.RenderBattleScreen(_player, _monster, _isEnd);
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
                    bool isItemUsed = TryUseItem();

                    if (isItemUsed && !_monster.IsDead)
                    {
                        MonsterAttack();
                    }
                    break;
                case 4:
                    _isEscaped = true;
                    ConsoleRenderer.AddLog("전투에서 도망쳤습니다.");
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

        private bool TryUseItem()
        {
            string currentInput = "";

            while (true)
            {
                // 인벤토리 화면 렌더링 (전투 전용 가이드 텍스트 전달)
                ViewRenderer.RenderInventory(_player, currentInput, "인벤토리", "사용할 아이템의 번호를 입력하세요. (Q: 취소)");

                // 문자 입력 받기
                var keyInfo = Console.ReadKey(true);

                // 0번을 누르면 취소
                if (keyInfo.Key == ConsoleKey.Q)
                {
                    return false;
                }

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (int.TryParse(currentInput, out int choice))
                    {
                        var validSlots = _player.Inventory.Slots
                            .Where(s => !s.Value.IsEmpty)
                            .ToList();

                        // 입력한 번호가 유효한지 확인
                        if (choice > 0 && choice <= validSlots.Count)
                        {
                            var targetSlot = validSlots[choice - 1];
                            int slotKey = targetSlot.Key;           // 딕셔너리의 Key
                            Item item = targetSlot.Value.ItemData;  // 실제 아이템 데이터

                            // 소비 아이템인지 체크
                            if (item is Consumable consumable)
                            {
                                bool isSuccess = consumable.UseItem(_player);

                                if (isSuccess)
                                {
                                    ConsoleRenderer.AddLog($"{item.Name} 사용!");
                                    // 사용 성공 시 인벤토리에서 아이템 1개 제거
                                    _player.Inventory.RemoveItem(slotKey, 1);
                                    return true; // 턴 소모됨을 알림
                                }
                            }
                            else
                            {
                                ConsoleRenderer.AddLog($"{item.Name}은(는) 전투 중에 사용할 수 없습니다.");
                            }
                        }
                        else
                        {
                            ConsoleRenderer.AddLog("잘못된 번호입니다.");
                        }
                    }
                    currentInput = ""; // 잘못 입력하거나 사용할 수 없는 아이템인 경우 입력창 초기화
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && currentInput.Length > 0)
                {
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                }
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    currentInput += keyInfo.KeyChar;
                }
            }
        }
    }
}