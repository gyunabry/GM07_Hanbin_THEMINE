using System;
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
                else if (mapManager.CurrentMap == Map.Inventory)
                {
                    RunInventory(mapManager, player);
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
                    mapManager.MoveToOtherMap(Map.Inventory);
                }
            }
        }

        private static void RunShop(MapManager mapManager, ShopManager shopManager, Player player)
        {
            ConsoleRenderer.ClearLogs();
            mapManager.MoveToShopMenu(Shop.Menu);
            ConsoleRenderer.AddLog("상점에 입장했습니다.");

            bool inShop = true;
            StringBuilder inputBuffer = new StringBuilder();

            while (inShop)
            {
                if (mapManager.CurrentShop == Shop.Menu)
                {
                    ViewRenderer.RenderShopMenu(player, inputBuffer.ToString());
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        inShop = false;
                    }
                    else if (keyInfo.KeyChar == '1')
                    {
                        mapManager.MoveToShopMenu(Shop.Buy);
                        ConsoleRenderer.ClearLogs();
                    }
                    else if (keyInfo.KeyChar == '2')
                    {
                        mapManager.MoveToShopMenu(Shop.Sell);
                        ConsoleRenderer.ClearLogs();
                    }
                }
                // 1번 선택, 상점 구매 페이지
                else if (mapManager.CurrentShop == Shop.Buy)
                {
                    ViewRenderer.RenderShopBuy(shopManager, player, inputBuffer.ToString());
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        mapManager.MoveToShopMenu(Shop.Menu);
                        inputBuffer.Clear();
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
                // 2번 선택, 아이템 판매
                else if (mapManager.CurrentShop == Shop.Sell)
                {
                    ViewRenderer.RenderInventory(
                        player,
                        inputBuffer.ToString(),
                        "상점 - 판매",
                        "판매하실 아이템의 [번호]를 입력하세요. (Q : 상점 메뉴로 돌아가기)"
                    );

                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.Q)
                    {
                        mapManager.MoveToShopMenu(Shop.Menu); // 메뉴로 뒤로가기
                        inputBuffer.Clear();
                    }
                    else if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        if (int.TryParse(inputBuffer.ToString(), out int itemNumber))
                        {
                            // 화면에 출력된 인벤토리 순서와 동일하게 아이템 목록을 가져옴
                            var validSlots = player.Inventory.Slots
                                .Where(s => !s.Value.IsEmpty)
                                .ToList();

                            int targetIndex = itemNumber - 1;
                            if (targetIndex >= 0 && targetIndex < validSlots.Count)
                            {
                                // 판매할 아이템의 실제 인벤토리 Key(슬롯 번호)를 찾아 판매
                                int slotKey = validSlots[targetIndex].Key;
                                shopManager.SellItem(slotKey, player);
                            }
                            else
                            {
                                ConsoleRenderer.AddLog("잘못된 번호입니다.");
                            }
                        }
                        inputBuffer.Clear();
                    }
                    else if (keyInfo.Key == ConsoleKey.Backspace && inputBuffer.Length > 0)
                    {
                        inputBuffer.Remove(inputBuffer.Length - 1, 1);
                    }
                    else if (char.IsDigit(keyInfo.KeyChar) && inputBuffer.Length < 2)
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

            // 플레이어가 죽지 않는 한 BattleManager의 StartBattle 메서드가 계속 실행됨
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

        private static void RunInventory(MapManager mapManager, Player player)
        {
            ConsoleRenderer.ClearLogs();

            bool inEquipment = true;
            StringBuilder inputBuffer = new StringBuilder();

            while (inEquipment)
            {
                ViewRenderer.RenderInventory(player, 
                    inputBuffer.ToString(), 
                    "인벤토리", 
                    "장착/해제할 장비 혹은 사용할 아이템의 [번호]를 입력하세요. (Q : 돌아가기)"
                );
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
                        var validSlots = player.Inventory.Slots
                            .Where(s => !s.Value.IsEmpty)
                            .ToList();

                        int targetIndex = itemNumber - 1;
                        if (targetIndex >= 0 && targetIndex < validSlots.Count)
                        {
                            var targetItem = validSlots[targetIndex].Value.ItemData;

                            // 아이템 타입에 맞게 로직 분리
                            if (targetItem is Equipment eq)
                            {
                                player.ToggleEquip(eq);
                            }
                            else if (targetItem is Consumable consumable)
                            {
                                // TODO: 물약 사용 로직
                                ConsoleRenderer.AddLog($"{consumable.Name}을(를) 사용했습니다.");
                            }
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
