using System;
using System.Text;

namespace TheMine
{
    // 아이템, 스킬, 적 등의 정보를 저장할 클래스
    class DataManager
    {
        // 싱글톤 인스턴스
        public static DataManager Instance { get; } = new DataManager();

        public Dictionary<string, Pickaxe> pickaxeDict = new Dictionary<string, Pickaxe>();
        public Dictionary<string, Armor> armorDict = new Dictionary<string, Armor>();
        public Dictionary<string, MonsterData> monsterDict = new Dictionary<string, MonsterData>();

        private Random _random = new Random();

        private DataManager() { }

        // 장비 CSV 읽기 메서드
        public void ReadEquipmentCsv(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"파일을 찾을 수 없습니다: {filePath}");
                return;
            }

            pickaxeDict.Clear();
            armorDict.Clear();

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] row = line.Split(',');

                if (row.Length < 10) continue;

                string type = row[0].ToLower();

                if (type == "pickaxe")
                {
                    Pickaxe pickaxe = new Pickaxe
                    {
                        Id = row[1],
                        Name = row[2],
                        Durability = int.Parse(row[3]),
                        BaseMiningPower = int.Parse(row[4]),
                        UpgradeValue = int.Parse(row[5]),
                        CritChance = int.Parse(row[6]),
                        Price = int.Parse(row[7]),
                        SellPrice = int.Parse(row[8]),
                        Description = row[9]
                    };
                    pickaxeDict[pickaxe.Id] = pickaxe;
                }
                else if (type == "armor")
                {
                    Armor armor = new Armor
                    {
                        Id = row[1],
                        Name = row[2],
                        Durability = int.Parse(row[3]),
                        BaseDefense = int.Parse(row[4]),
                        UpgradeValue = int.Parse(row[5]),
                        Price = int.Parse(row[7]),
                        SellPrice = int.Parse(row[8]),
                        Description = row[9]
                    };
                    armorDict[armor.Id] = armor;
                }
            }
        }

        // 몬스터 CSV 읽기 메서드
        public void ReadMonsterCsv(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"파일을 찾을 수 없습니다: {filePath}");
                return;
            }

            monsterDict.Clear();

            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] row = line.Split(',');
                if (row.Length < 8) continue;

                MonsterData monster = new MonsterData
                {
                    Id = row[0],
                    Name = row[1],
                    MaxHp = int.Parse(row[2]),
                    AttackPower = int.Parse(row[3]),
                    Def = int.Parse(row[4]),
                    DropGold = int.Parse(row[5]),
                    DropExp = int.Parse(row[6]),
                    Description = row[7]
                };

                monsterDict[monster.Id] = monster;
            }
        }

        public void SaveData()
        {
            // 플레이어 상태 (이름, 레벨, 소지 골드, 인벤토리?) 인벤토리 직렬화?
        }
    }
}
