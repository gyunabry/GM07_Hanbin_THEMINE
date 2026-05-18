using System.Text;
using System;

namespace TheMine
{
    // 몬스터 스폰 클래스
    class SpawnManager
    {
        DataManager dm = DataManager.Instance;
        
        private Random _random = new Random();

        public Monster CreateMonster(string id)
        {
            if (!dm.monsterDict.TryGetValue(id, out MonsterData data))
            {
                throw new InvalidOperationException($"몬스터 데이터를 찾을 수 없습니다: {id}");
            }

            return new Monster(
                data.Id,
                data.Name,
                data.MaxHp,
                data.AttackPower,
                data.Def,
                data.DropGold,
                data.DropExp,
                data.Description);
        }

        public Monster CreateRandomMonster()
        {
            if (dm.monsterDict == null || dm.monsterDict.Count == 0)
            {
                throw new Exception("몬스터 없음");
            }

            // 랜덤 선택
            // TODO: 몬스터별 가중치를 두어 가중치에 따른 스폰 확률 설정
            int random = _random.Next(0, dm.monsterDict.Count);

            MonsterData randomData = dm.monsterDict.Values.ElementAt(random);

            return CreateMonster(randomData.Id);
        }
    }
}
