using System.Diagnostics;
using static System.Console;

namespace TheMine
{
    // 콘솔창을 꾸미기 위한 클래스
    public class ConsoleRenderer
    {
        private static int width;           // 메인화면 가로
        private static int sideWidth;       // 사이드 화면 가로 크기
        private static int height;          // 메인화면 세로
        private static int bottomHeight;    // 하단바 크기

        // 게임 로그 큐
        private static Queue<string> gameLogs = new Queue<string>();
        private static int maxLogCount = 12;

        private static char[,]? currentBuffer;
        private static char[,]? nextBuffer;
        private static int bufferWidth;
        private static int bufferHeight;
        private static bool isBuffering;

        /// <summary>
        /// 처음 시작 시 콘솔 설정
        /// </summary>
        public static void Initialize()
        {
            OutputEncoding = System.Text.Encoding.UTF8;

            Title = "THE MINE";
            ForegroundColor = ConsoleColor.White;
            CursorVisible = false;

            width = 75;
            sideWidth = 30;
            height = 30;
            bottomHeight = 3;

            SetWindowSize(width + sideWidth + 1, height + bottomHeight + 1);
            // SetBufferSize(width + sideWidth + 1, height + bottomHeight + 1);

            InitializeBuffer();
            Clear();
        }

        /// <summary>
        /// 처음 시작 시 타이틀 보여주고
        /// 게임 기록이 있다면 로딩바, 없다면 닉네임 입력 후 로딩바
        /// </summary>
        public static void ShowMainMenu()
        {
            int totalWidth = width + sideWidth - 1;

            PrintCenteredBlock(5, DrawTitle(), totalWidth);

            // DataManager에서 기존 데이터 있는지 검사 후 로딩바 출력
        }

        public static string InputPlayerName()
        {
            string title = "플레이어 이름을 입력하세요";
            string arrow = "> ";
            int totalWidth = width + sideWidth - 1;
            int inputX = Math.Max(0, (totalWidth - 20) / 2);
            int inputY = (height + bottomHeight) / 2;

            CursorVisible = true;
            PrintCenteredBlock(inputY - 10, DrawTitle(), totalWidth);
            PrintCentered(inputY - 1, title, totalWidth);

            SetCursorPosition(inputX, inputY + 1);
            Write(arrow);

            string input = ReadLine();
            string playerName = string.IsNullOrEmpty(input) ? "플레이어" : input.Trim();

            CursorVisible = false;
            Clear();
            ResetBuffer();

            DrawFrame();

            return playerName;
        }

        // 콘솔창 그리기 메서드
        public static void DrawFrame()
        {
            // 좌측 상단
            SetCursorPosition(0, 0);
            Write("┏" + new string('━', width - 2) + "┳" + new string('━', sideWidth - 2) + "┓");

            // 중간 벽면 및 분리선
            for (int i = 1; i < height + bottomHeight - 1; i++)
            {
                SetCursorPosition(0, i);
                Write("┃");
                SetCursorPosition(width - 1, i);
                Write("┃");
                SetCursorPosition(width + sideWidth - 2, i);
                Write("┃");
            }

            int splitY = GetBottomSplitY();
            if (splitY > 1 && splitY < height - 2)
            {
                SetCursorPosition(0, splitY);
                Write("┣");

                SetCursorPosition(1, splitY);
                Write(new string('━', width - 2));

                SetCursorPosition(width - 1, splitY);
                Write("┫");
            }

            int sideSplitY = 14;
            SetCursorPosition(width - 1, sideSplitY);
            Write("┣" + new string('━', sideWidth - 2) + "┫");

            // 하단 테두리
            SetCursorPosition(0, height + bottomHeight - 1);
            Write("┗" + new string('━', width - 2) + "┻" + new string('━', sideWidth - 2) + "┛");
        }

        #region 출력 버퍼
        public static void BeginBuffer()
        {
            EnsureBuffer();
            isBuffering = true;

            for (int y = 0; y < bufferHeight; y++)
            {
                for (int x = 0; x < bufferWidth; x++)
                {
                    nextBuffer![x, y] = ' ';
                }
            }
        }

        public static void DrawFrameToBuffer()
        {
            // 좌측 상단 및 외곽선 상단
            BufferAt(0, 0, "┏" + new string('━', width - 2) + "┳" + new string('━', sideWidth - 2) + "┓");

            // 중간 벽면
            for (int y = 1; y < height + bottomHeight - 1; y++)
            {
                BufferAt(0, y, "┃");
                BufferAt(width - 1, y, "┃");
                BufferAt(width + sideWidth - 2, y, "┃");
            }

            // 메인화면 하단 분리선 (선택지 UI 구분선)
            int splitY = GetBottomSplitY();
            if (splitY > 1 && splitY < height - 2)
            {
                BufferAt(0, splitY, "┣" + new string('━', width - 2) + "┫");
            }

            // 사이드 화면(상태창 / 인벤토리) 분리선
            int sideSplitY = 14;
            BufferAt(width - 1, sideSplitY, "┣" + new string('━', sideWidth - 2) + "┫");

            // 하단 테두리
            BufferAt(0, height + bottomHeight - 1, "┗" + new string('━', width - 2) + "┻" + new string('━', sideWidth - 2) + "┛");
        }

        public static void BufferAt(int x, int y, string text)
        {
            EnsureBuffer();

            if (y < 0 || y >= bufferHeight)
            {
                return;
            }

            int cursorX = x;
            for (int i = 0; i < text.Length; i++)
            {
                if (cursorX < 0 || cursorX >= bufferWidth)
                {
                    cursorX += IsKorean(text[i]) ? 2 : 1;
                    continue;
                }

                nextBuffer![cursorX, y] = text[i];

                if (IsKorean(text[i]) && cursorX + 1 < bufferWidth)
                {
                    nextBuffer[cursorX + 1, y] = '\0';
                }

                cursorX += IsKorean(text[i]) ? 2 : 1;
            }
        }

        public static void EndBuffer()
        {
            EnsureBuffer();

            for (int y = 0; y < bufferHeight; y++)
            {
                for (int x = 0; x < bufferWidth; x++)
                {
                    if (nextBuffer![x, y] == '\0')
                    {
                        currentBuffer![x, y] = '\0';
                        continue;
                    }

                    if (currentBuffer![x, y] == nextBuffer[x, y])
                    {
                        continue;
                    }

                    SetCursorPosition(x, y);
                    Write(nextBuffer[x, y]);
                    currentBuffer[x, y] = nextBuffer[x, y];

                    if (IsKorean(nextBuffer[x, y]) && x + 1 < bufferWidth)
                    {
                        currentBuffer[x + 1, y] = '\0';
                    }
                }
            }

            isBuffering = false;
        }

        public static void ResetBuffer()
        {
            EnsureBuffer();

            for (int y = 0; y < bufferHeight; y++)
            {
                for (int x = 0; x < bufferWidth; x++)
                {
                    currentBuffer![x, y] = '\0';
                    nextBuffer![x, y] = ' ';
                }
            }
        }
        #endregion

        // 처음 시작 시 보여줄 화면
        public static string DrawTitle()
        {
            return "████████╗██╗  ██╗███████╗███╗   ███╗██╗███╗   ██╗███████╗\r\n╚══██╔══╝██║  ██║██╔════╝████╗ ████║██║████╗  ██║██╔════╝\r\n   ██║   ███████║█████╗  ██╔████╔██║██║██╔██╗ ██║█████╗  \r\n   ██║   ██╔══██║██╔══╝  ██║╚██╔╝██║██║██║╚██╗██║██╔══╝  \r\n   ██║   ██║  ██║███████╗██║ ╚═╝ ██║██║██║ ╚████║███████╗\r\n   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝╚══════╝\r\n                                                         ";
        }

        internal static void RenderBattleScreen(Player player, Monster monster, bool isEnd)
        {
            BeginBuffer();
            DrawFrameToBuffer();
            PrintLogs();
            // PrintBattleOptions();
            if (!isEnd)
            {
                PrintOptions($"1. 기본 공격\t2. 스킬\t3. 아이템 사용\t4. 도망");
            }
            else
            {
                PrintOptions($"1. 더 깊은 곳으로 내려간다\t2. 마을로 귀환");
            }
            PrintStatusPanel(player);
            PrintMonsterState(monster);
            PrintInventory(width + 2, 16, player.Inventory);
            EndBuffer();
        }

        internal static void RenderAfterBattleScreen(Player player, Monster monster)
        {
            BeginBuffer();
            DrawFrameToBuffer();
            PrintLogs();
            // PrintAfterBattleOptions();
            PrintStatusPanel(player);
            PrintMonsterState(monster);
            PrintInventory(width + 2, 16, player.Inventory);
            EndBuffer();
        }

        internal static int ReadBattleChoice()
        {
            int splitY = GetBottomSplitY();
            PrintLine(2, splitY + 2, "선택 > ", width - 4);

            while (true)
            {
                ConsoleKeyInfo key = ReadKey(true);
                if (key.KeyChar >= '1' && key.KeyChar <= '4')
                {
                    return key.KeyChar - '0';
                }
            }
        }

        internal static void PrintOptions(string options)
        {
            options = options.Replace("\t", "    ");
            int splitY = GetBottomSplitY();
            ClearRegion(1, splitY + 1, width - 3, bottomHeight - 1);
            PrintLine(2, splitY + 1, options, width - 4);
        }

        internal static int ReadAfterBattleChoice()
        {
            int splitY = GetBottomSplitY();
            PrintLine(2, splitY + 2, "선택 > ", width - 4);

            while (true)
            {
                ConsoleKeyInfo key = ReadKey(true);
                // 1번(계속 진행) 또는 2번(마을 귀환)만 입력받음
                if (key.KeyChar == '1' || key.KeyChar == '2')
                {
                    return key.KeyChar - '0';
                }
            }
        }

        internal static void PrintStatusPanel(Player player)
        {
            int x = width + 2;
            int y = 2;

            PrintCenteredInSide(y, "상태창");
            PrintLine(x, y + 3, $"플레이어: {player.Name}", sideWidth - 4);
            PrintLine(x, y + 4, $"HP {player.Hp} / {player.MaxHp}", sideWidth - 4);
            PrintLine(x, y + 5, $"MP {player.Mp} / {player.MaxMp}", sideWidth - 4);
            PrintLine(x, y + 6, $"공격력 {player.TotalAttackPower}", sideWidth - 4);
            PrintLine(x, y + 7, $"방어력 {player.TotalDefense}", sideWidth - 4);

            PrintLine(x, y + 8, $"소지금액 {player.Gold}", sideWidth - 4);
            PrintLine(x, y + 9, $"경험치 {player.Exp} / {player.GetRequiredExpForNextLevel()}", sideWidth - 4);
        }

        internal static void PrintMonsterState(Monster monster)
        {
            int x = width / 2;
            int y = 2;

            PrintCentered(y, $"지하 {GameManager.Instance.CurrentDepth.ToString()}층", width);
            PrintCentered(y + 2, monster.Name, width);
            PrintCentered(y + 3, $"[{monster.Hp} / {monster.MaxHp}]", width);
        }

        // 인벤토리 표시 메서드
        internal static void PrintInventory(int startX, int startY, Inventory inventory)
        {
            const int nameWidth = 14;

            PrintLine(startX, startY, "인벤토리", sideWidth - 4);
            PrintLine(startX, startY + 1, "No  " + PadRightForPrintLength("이름", nameWidth) + "수량", sideWidth - 4);
            PrintLine(startX, startY + 2, "--------------------------", sideWidth - 4);

            if (inventory == null)
            {
                return;
            }

            int yOffset = 3;
            foreach (var slot in inventory.Slots.OrderBy(slot => slot.Key))
            {
                if (slot.Value.IsEmpty)
                {
                    continue;
                }

                string itemName = slot.Value.ItemData.Name;
                
                // 장착 중인 장비라면 이름 앞에 [E] 붙임
                if (slot.Value.ItemData is Equipment eq && eq.IsEquipped)
                {
                    itemName = "[E]" + itemName;
                }
                string count = $"x{slot.Value.Count}";
                string line = $"{slot.Key,-3} {PadRightForPrintLength(itemName, nameWidth)}{count}";

                PrintLine(startX, startY + yOffset, line, sideWidth - 4);
                yOffset++;
            }
        }

        public static void AddLog(string msg)
        {
            gameLogs.Enqueue(msg);
            if (gameLogs.Count > maxLogCount)
            {
                gameLogs.Dequeue();
            }
        }

        public static void ClearLogs()
        {
            gameLogs.Clear();
        }

        public static void PrintLogs()
        {
            int startY = 20; // 선택지 UI 바로 위
            int maxWidth = width - 4;
            int visibleRows = GetBottomSplitY() - startY - 1;
            string[] logs = gameLogs.TakeLast(visibleRows).ToArray();

            ClearRegion(2, startY, width - 4, GetBottomSplitY() - startY);

            for (int i = 0; i < logs.Length; i++)
            {
                PrintLine(2, startY + i, logs[i], maxWidth);
            }
        }

        #region 유틸
            // 전각문자 대응용
        private static bool IsKorean(char c) => (c >= '\uAC00' && c <= '\uD7A3') || (c >= '\u1100' && c <= '\u11FF');

        public static int GetPrintingLength(string line) => line.Sum(c => IsKorean(c) ? 2 : 1);

        public static void PrintAt(int x, int y, string text)
        {
            if (isBuffering)
            {
                BufferAt(x, y, text);
                return;
            }

            SetCursorPosition(x, y);
            Write(text);
        }

        private static void InitializeBuffer()
        {
            bufferWidth = width + sideWidth - 1;
            bufferHeight = height + bottomHeight;
            currentBuffer = new char[bufferWidth, bufferHeight];
            nextBuffer = new char[bufferWidth, bufferHeight];
            ResetBuffer();
        }

        private static void EnsureBuffer()
        {
            if (currentBuffer == null || nextBuffer == null)
            {
                InitializeBuffer();
            }
        }

        private static void PrintLine(int x, int y, string text, int maxWidth)
        {
            if (isBuffering)
            {
                string trimmedText = TrimToPrintLength(text, maxWidth);
                int bufferPadding = Math.Max(0, maxWidth - GetPrintingLength(text));
                BufferAt(x, y, trimmedText + new string(' ', bufferPadding));
                return;
            }

            SetCursorPosition(x, y);
            Write(TrimToPrintLength(text, maxWidth));
            int padding = Math.Max(0, maxWidth - GetPrintingLength(text));
            Write(new string(' ', padding));
        }

        public static void ClearRegion(int x, int y, int regionWidth, int regionHeight)
        {
            for (int row = 0; row < regionHeight; row++)
            {
                if (isBuffering)
                {
                    BufferAt(x, y + row, new string(' ', regionWidth));
                }
                else
                {
                    SetCursorPosition(x, y + row);
                    Write(new string(' ', regionWidth));
                }
            }
        }

        private static void PrintCentered(int y, string text, int totalWidth)
        {
            int x = Math.Max(0, (totalWidth - GetPrintingLength(text)) / 2);
            PrintAt(x, y, text);
        }

        public static void PrintCenteredInSide(int y, string text)
        {
            int x = width + Math.Max(0, (sideWidth - GetPrintingLength(text)) / 2);
            PrintAt(x, y, text);
        }

        private static void PrintCenteredBlock(int startY, string text, int totalWidth)
        {
            string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                int x = Math.Max(0, (totalWidth - GetPrintingLength(lines[i])) / 2);
                PrintAt(x, startY + i, lines[i]);
            }
        }

        public static string PadRightForPrintLength(string text, int totalWidth)
        {
            int padding = Math.Max(0, totalWidth - GetPrintingLength(text));
            return text + new string(' ', padding);
        }

        private static string TrimToPrintLength(string text, int maxWidth)
        {
            int length = 0;
            List<char> chars = new List<char>();

            foreach (char c in text)
            {
                int charLength = IsKorean(c) ? 2 : 1;
                if (length + charLength > maxWidth)
                {
                    break;
                }

                chars.Add(c);
                length += charLength;
            }

            return new string(chars.ToArray());
        }

        public static int GetBottomSplitY()
        {
            return height - bottomHeight - 2;
        }
        #endregion
    }
}
