namespace TheMine
{
    // View UI를 채울 렌더러
    class ViewRenderer
    {
        private const int MainAreaWidth = 73;
        private const int MainAreaHeight = 24;
        private const int MainAreaStartX = 1;
        private const int MainAreaStartY = 1;

        public static void RenderTown(MapManager mapManager, Player player)
        {
            ConsoleRenderer.BeginBuffer();
            ConsoleRenderer.DrawFrameToBuffer();

            ConsoleRenderer.ClearRegion(2, 2, 71, 22);

            ConsoleRenderer.BufferAt(33, 5, "마을");
            ConsoleRenderer.BufferAt(20, 8, "평화로운 마을이다. 무엇을 할까?");

            ConsoleRenderer.PrintOptions($"1. 상점 입장 \t2. 던전 입장 \t3. 인벤토리 확인");

            ConsoleRenderer.PrintStatusPanel(player);
            ConsoleRenderer.PrintInventory(77, 16, player.Inventory);

            ConsoleRenderer.EndBuffer();
        }

        public static void RenderShopMenu(Player player, string currentInput)
        {
            ConsoleRenderer.BeginBuffer();
            ConsoleRenderer.DrawFrameToBuffer();

            ConsoleRenderer.ClearRegion(2, 2, 71, 22);

            // 상단 타이틀
            ConsoleRenderer.BufferAt(33, 4, "상점");
            ConsoleRenderer.BufferAt(20, 8, "상점에 들어왔다. 무엇을 할까?");

            ConsoleRenderer.BufferAt(26, 12, "1. 아이템 구매");
            ConsoleRenderer.BufferAt(26, 14, "2. 아이템 판매");

            ConsoleRenderer.PrintLogs();

            int bottomY = ConsoleRenderer.GetBottomSplitY();
            ConsoleRenderer.BufferAt(3, bottomY + 1, "원하시는 기능의 [번호]를 입력하세요. (Q: 마을로 돌아가기)");
            ConsoleRenderer.BufferAt(3, bottomY + 2, $">> 입력 중 : {currentInput}");

            // 우측 영역에 플레이어 상태창 및 인벤토리 실시간 유지
            ConsoleRenderer.PrintStatusPanel(player);
            ConsoleRenderer.PrintInventory(77, 16, player.Inventory);

            ConsoleRenderer.EndBuffer();
        }

        public static void RenderShopBuy(ShopManager shopManager, Player player, string currentInput)
        {
            ConsoleRenderer.BeginBuffer();
            ConsoleRenderer.DrawFrameToBuffer();

            ConsoleRenderer.ClearRegion(2, 2, 71, 22);

            // 상단 타이틀
            ConsoleRenderer.BufferAt(33, 2, "상점");
            ConsoleRenderer.BufferAt(3, 4, "---------------------------------------------------------------------");
            ConsoleRenderer.BufferAt(3, 5, " 번호 |      아이템 이름      |  가격  |          설명          ");
            ConsoleRenderer.BufferAt(3, 6, "---------------------------------------------------------------------");

            int startY = 7;
            var items = shopManager.ShopItems;

            for (int i = 0; i < items.Count; i++)
            {
                int currentY = startY + i;
                if (currentY >= 20) break; // 화면 바깥으로 나가는 것 방지

                string numberStr = string.Format(" [{0:D2}]", i + 1);
                // 이름칸 길이를 맞추기 위한 포맷팅 (정렬 깨짐 방지용 정렬 문자열 배치 가능)
                string nameStr = items[i].Name.PadRight(20);
                string priceStr = string.Format("{0,5} G", items[i].Price);
                string descStr = items[i].Description;

                // 콘솔 버퍼에 정렬하여 출력
                ConsoleRenderer.BufferAt(3, currentY, numberStr);
                ConsoleRenderer.BufferAt(10, currentY, nameStr);
                ConsoleRenderer.BufferAt(35, currentY, priceStr);
                ConsoleRenderer.BufferAt(45, currentY, descStr);
            }
            ConsoleRenderer.PrintLogs();

            int bottomY = ConsoleRenderer.GetBottomSplitY();
            ConsoleRenderer.BufferAt(3, bottomY + 1, "구매하실 아이템의 [번호]를 입력하세요. (Q: 마을로 돌아가기)");
            ConsoleRenderer.BufferAt(3, bottomY + 2, $">> 입력 중 : {currentInput} (Enter를 누르면 아이템이 구매됩니다.)");

            // 우측 영역에 플레이어 상태창 및 인벤토리 실시간 유지
            ConsoleRenderer.PrintStatusPanel(player);
            ConsoleRenderer.PrintInventory(77, 16, player.Inventory);

            ConsoleRenderer.EndBuffer();
        }

        public static void RenderShopSell(ShopManager shopManager, Player player, string currentInput)
        {

        }

        public static void RenderInventory(Player player, string currentInput, string title, string guideText)
        {
            ConsoleRenderer.BeginBuffer();
            ConsoleRenderer.DrawFrameToBuffer();
            ConsoleRenderer.ClearRegion(2, 2, 71, 22);

            ConsoleRenderer.BufferAt(31, 2, "장비 관리");
            ConsoleRenderer.BufferAt(3, 4, "---------------------------------------------------------------------");
            ConsoleRenderer.BufferAt(3, 5, " 번호 |      장비 이름      |  능력치  |          설명          ");
            ConsoleRenderer.BufferAt(3, 6, "---------------------------------------------------------------------");

            int startY = 7;

            // 인벤토리에서 아이템이 있는 칸만 필터링해서 리스트로 추출
            var validSlots = player.Inventory.Slots
                .Where(s => !s.Value.IsEmpty)
                .ToList();

            for (int i = 0; i < validSlots.Count; i++)
            {
                int currentY = startY + i;
                if (currentY >= 20) break;

                var slot = validSlots[i];
                var item = slot.Value.ItemData;
                int count = slot.Value.Count;

                // 인덱스
                string numberStr = string.Format(" [{0:D2}]", i + 1);

                // 아이템 이름
                string nameStr = item.Name;
                if (item is Equipment eq && eq.IsEquipped)
                {
                    nameStr = "[E]" + nameStr;
                }
                nameStr = ConsoleRenderer.PadRightForPrintLength(nameStr, 20);

                // 스탯은 장비만 출력
                string statStr = "";
                if (item is Pickaxe pickaxe)
                    statStr = $"공격력 {pickaxe.BaseMiningPower}";
                else if (item is Armor armor)
                    statStr = $"방어력 {armor.BaseDefense}";
                statStr = ConsoleRenderer.PadRightForPrintLength(statStr, 20);

                // 아이템 수량
                string countStr = string.Format("x{0}", count);

                ConsoleRenderer.BufferAt(3, currentY, numberStr);
                ConsoleRenderer.BufferAt(10, currentY, nameStr);
                ConsoleRenderer.BufferAt(32, currentY, statStr);
                ConsoleRenderer.BufferAt(58, currentY, countStr);
            }

            ConsoleRenderer.PrintLogs();

            int bottomY = ConsoleRenderer.GetBottomSplitY();
            ConsoleRenderer.BufferAt(3, bottomY + 1, guideText);
            ConsoleRenderer.BufferAt(3, bottomY + 2, $">> 입력 중 : {currentInput} (Enter를 누르면 적용됩니다.)");

            ConsoleRenderer.PrintStatusPanel(player);
            ConsoleRenderer.PrintInventory(77, 16, player.Inventory);

            ConsoleRenderer.EndBuffer();
        }
    }
}
