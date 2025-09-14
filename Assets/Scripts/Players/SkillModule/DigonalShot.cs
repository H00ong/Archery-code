using Game.Player.Attack;
using System.Collections.Generic;
using UnityEngine;

public class DiagonalShot : PlayerSkillModuleBase, IShootContributor
{
    [SerializeField, Min(0f)] private float interval = 2f; // 좌우 최대 오프셋
    [SerializeField, Min(0f)] private float angle = 40f;     // 양 끝 총알의 최대 회전 각도(±)

    private int GetBulletCount(int lvl) => Mathf.Max(2 * lvl, 2); // 항상 짝수(중앙 없음)

    // normalized vector3가 온다고 가정
    public void AddBullet(List<ShotInstruction> bulletList, ShotInstruction _default)
    {
        if (bulletList == null) return;

        int count = GetBulletCount(Level);               // 예: level=1 → 2발, 2 → 4발 …
        int pairs = count / 2;                           // 좌우 쌍 개수
        if (pairs <= 0) return;

        // 용량 예열
        int need = bulletList.Count + count;
        if (bulletList.Capacity < need) bulletList.Capacity = need;

        Vector3 origin = _default.shootingPos;
        Vector3 dir0 = _default.shootingDir;
        Vector3 up     = Vector3.up;          // 로컬 업 축 기준 회전
        Vector3 right  = Vector3.Cross(up, dir0);

        // 인덱스를 [-pairs..-1, 1..pairs]로 매핑 → 중앙(0) 제외
        for (int i = 0; i < count; i++)
        {
            int k = i - (i < pairs ? pairs : pairs - 1); // [-pairs..-1, 1..pairs]
            float t = k / (float)pairs;                  // 정규화 오프셋: [-1..-1/pairs]∪[1/pairs..1]

            float offset = t * interval;                 // 좌우 위치: [-interval..interval]
            Vector3 pos = origin + right * offset;

            // 각도도 정규화하여 극단이 ±angle 되도록
            Quaternion rot = Quaternion.AngleAxis(angle * t, up);
            Vector3 dir = rot * dir0;

            bulletList.Add(new ShotInstruction
            {
                shootingDir = dir,   // 반드시 회전 적용된 방향 사용
                shootingPos = pos
            });
        }
    }
}

