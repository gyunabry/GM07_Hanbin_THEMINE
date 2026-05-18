using System.Text;

namespace TheMine
{
    class GameManager
    {
        public static GameManager Instance { get; } = new GameManager();

        public int CurrentDepth { get; set; } = 1;

        public static void Run()
        {
            ConsoleRenderer.Initialize();

            DataManager.Instance.ReadEquipmentCsv("Data\\EquipmentData.csv");
            DataManager.Instance.ReadMonsterCsv("Data\\MonsterData.csv");

            // 메인메뉴 (플레이어 이름 입력 후 마을로 이동)
            ConsoleRenderer.ShowMainMenu();
            string playerName = ConsoleRenderer.InputPlayerName();

            Player player = new Player(playerName, 200, 50, 30, 5);
            player.Inventory = new Inventory();

            MapManager mapManager = new MapManager();
            SpawnManager spawnManager = new SpawnManager();
            ShopManager shopManager = new ShopManager();
            shopManager.InitShop();

            while (!player.IsDead)
            {
                RunTown(player, mapManager);

                if (mapManager.CurrentMap == Map.Shop)
                {
                    RunShop(mapManager, shopManager, player);
                }
                else if (mapManager.CurrentMap == Map.Dungeon)
                {
                    RunDungeon(player, mapManager, spawnManager);
                }
                else if (mapManager.CurrentMap == Map.Equipment)
                {
                    RunEquipment(mapManager, player);
                }
            }
        }

        private static void RunTown(Player player, MapManager mapManager)
        {
            mapManager.ReturnToTown();

            while (mapManager.CurrentMap == Map.Town)
            {
                ViewRenderer.RenderTown(mapManager, player);

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.KeyChar == '1')
                {
                    mapManager.MoveToOtherMap(Map.Shop);
                }
                else if (keyInfo.KeyChar == '2')
                {
                    mapManager.MoveToOtherMap(Map.Dungeon);
                }
                else if (keyInfo.KeyChar == '3')
                {
                    mapManager.MoveToOtherMap(Map.Equipment);
                }
            }
        }

        private static void RunShop(MapManager mapManager, ShopManager shopManager, Player player)
        {
            ConsoleRenderer.ClearLogs();
            ConsoleRenderer.AddLog("상점에 입장했습니다.");

            bool inShop = true;
            StringBuilder inputBuffer = new StringBuilder();

            while (inShop)
            {
                ViewRenderer.RenderShop(shopManager, player, inputBuffer.ToString());
                int bottomY = ConsoleRenderer.GetBottomSplitY();
                // ConsoleRenderer.BufferAt(3, bottomY + 1, $">> 입력 중 : {inputBuffer} (Enter를 누르면 아이템이 구매됩니다.)");

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Q)
                {
                    inShop = false;
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (int.TryParse(inputBuffer.ToString(), out int itemNumber))
                    {
                        shopManager.BuyItem(itemNumber - 1, player);
                    }
                    else
                    {
                        ConsoleRenderer.AddLog("아이템 구매 실패! 올바른 숫자를 입력해주세요.");
                    }
                    inputBuffer.Clear();
                    // ConsoleRenderer.PrintAt(3, 25, new string(' ', 50));
                }
                // 백스페이스 처리
                // 스트링빌더에서 끝에 하나 빼기
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (inputBuffer.Length > 0)
                    {
                        inputBuffer.Remove(inputBuffer.Length - 1, 1);
                    }
                }
                // 숫자키 입력 처리
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    if (inputBuffer.Length < 2)
                    {
                        inputBuffer.Append(keyInfo.KeyChar);
                    }
                }
            }
            mapManager.ReturnToTown();
        }

        private static void RunDungeon(Player player, MapManager mapManager, SpawnManager spawnManager)
        {
            ConsoleRenderer.ClearLogs();
            Instance.CurrentDepth = 1;
            ConsoleRenderer.AddLog($"던전에 입장했습니다.");

            bool inDungeon = true;
            Random rand = new Random();

            Monster monster;

            while (inDungeon && !player.IsDead)
            {
                bool isBossEncounter = Instance.CurrentDepth >= 50;

                if (isBossEncounter)
                {
                    ConsoleRenderer.AddLog($"경고! 지하 {Instance.CurrentDepth}층에서 강력한 보스의 기운이 느껴진다.");

                    // 임시로 마지막 데이터 사용
                    var bossData = DataManager.Instance.monsterDict.Values.Last();
                    monster = spawnManager.CreateMonster(bossData.Id);
                }
                else
                {
                    ConsoleRenderer.AddLog($"지하 {Instance.CurrentDepth}층에 도달했다.");
                    monster = spawnManager.CreateRandomMonster();
                }

                BattleManager battleManager = new BattleManager();
                battleManager.Init(player, monster);
                inDungeon = battleManager.StartBattle(); // Battle의 결과를 inDungeon에 반환해 던전 루프 종료 확인
            }

            if (player.IsDead)
            {
                // 플레이어의 상태를 복구하여 IsDead를 false로 만듭니다.
                player.RestorePlayerState(player);
                ConsoleRenderer.AddLog("정신을 잃었으나 마을사람들에게 무사히 구조되었습니다! (HP/MP 회복)");
            }
            Console.ReadKey(true);
            mapManager.ReturnToTown();
        }

        private static void RunEquipment(MapManager mapManager, Player player)
        {
            ConsoleRenderer.ClearLogs();

            bool inEquipment = true;
            StringBuilder inputBuffer = new StringBuilder();

            while (inEquipment)
            {
                ViewRenderer.RenderEquipment(player, inputBuffer.ToString());
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.Q)
                {
                    inEquipment = false;
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (int.TryParse(inputBuffer.ToString(), out int itemNumber))
                    {
                        var equipments = player.Inventory.Slots
                            .Where(s => !s.Value.IsEmpty && s.Value.ItemData is Equipment)
                            .Select(s => s.Value.ItemData as Equipment)
                            .ToList();

                        int targetIndex = itemNumber - 1;
                        if (targetIndex >= 0 && targetIndex < equipments.Count)
                        {
                            // 장착/해제 토글 실행
                            player.ToggleEquip(equipments[targetIndex]);
                        }
                        else
                        {
                            ConsoleRenderer.AddLog("잘못된 번호입니다.");
                        }
                    }
                    inputBuffer.Clear();
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (inputBuffer.Length > 0)
                    {
                        inputBuffer.Remove(inputBuffer.Length - 1, 1);
                    }
                }
                else if (char.IsDigit(keyInfo.KeyChar))
                {
                    if (inputBuffer.Length < 2)
                    {
                        inputBuffer.Append(keyInfo.KeyChar);
                    }
                }
            }
            mapManager.ReturnToTown();
        }
    }
}
