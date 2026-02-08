using System.Collections.Generic;
using System.Linq; // ★ LINQ 필수!
using UnityEngine; // ★ Unity 필수

public class Test : MonoBehaviour
{
    // 데이터 담을 클래스 (MonoBehaviour 상속 필요 없음)
    public class Monster
    {
        public string Name { get; set; }
        public string Element { get; set; }
        public int Level { get; set; }
    }

    void Start()
    {
        // 1. 데이터 준비
        List<Monster> myMonsters = new List<Monster>
        {
            new Monster
            {
                Name = "파이리",
                Element = "Fire",
                Level = 10,
            },
            new Monster
            {
                Name = "꼬부기",
                Element = "Water",
                Level = 12,
            },
            new Monster
            {
                Name = "리자몽",
                Element = "Fire",
                Level = 50,
            },
            new Monster
            {
                Name = "이상해씨",
                Element = "Grass",
                Level = 15,
            },
            new Monster
            {
                Name = "잉어킹",
                Element = "Water",
                Level = 5,
            },
        };

        // 2. GroupBy 실행 (속성별로 묶기)
        var monsterGroups = myMonsters.GroupBy(m => m.Element);

        Debug.Log("<color=yellow>=== 속성별 몬스터 도감 ===</color>");

        // 3. 결과 출력
        foreach (var group in monsterGroups)
        {
            // 로그를 깔끔하게 보기 위해 문자열을 합쳐서 한 번에 출력합니다.
            string logMessage = $"<b>[ {group.Key} 속성 ]</b> (총 {group.Count()}마리)\n";

            foreach (var monster in group)
            {
                logMessage += $" - 이름: {monster.Name}, 레벨: {monster.Level}\n";
            }

            // 그룹 단위로 로그 찍기
            Debug.Log(logMessage);
        }
    }
}
