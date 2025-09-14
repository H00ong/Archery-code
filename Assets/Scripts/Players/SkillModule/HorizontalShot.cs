using Game.Player.Attack;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditorInternal.ReorderableList;

public class HorizontalShot : PlayerSkillModuleBase, IShootContributor
{
    [SerializeField, Min(0f)] private float interval = 1f;

    // 레벨별 발사 개수 매핑 (예시)
    // level 1 -> 2발, level 2 -> 3발, 그 이상은 선형 증가 등 정책화
    private int GetBulletCount(int lvl)
    {
        return lvl + 1;
    }
    
    // normalized vector 가 온다고 가정
    public void AddBullet(List<ShotInstruction> bulletList, ShotInstruction _default)
    {
        if (bulletList == null) return;

        int count = GetBulletCount(Level);
        if (count <= 0) return;

        // 리스트 용량 예열(선택 사항: GC/재할당 줄이기)
        int need = bulletList.Count + count;
        if (bulletList.Capacity < need) bulletList.Capacity = need;

        Vector3 origin = _default.shootingPos;
        Vector3 dir = _default.shootingDir;
        Vector3 up = Vector3.up;          // 로컬 업 축 기준 회전
        Vector3 right = Vector3.Cross(up, dir);

        // 중심 정렬: i = 0..count-1, half = (count-1)/2
        // offset = (i - half) * interval  => 짝수면 ±0.5, ±1.5… / 홀수면 0, ±1, ±2…
        float half = (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offset = (i - half) * interval;
            Vector3 pos = origin + right * offset;

            bulletList.Add(new ShotInstruction
            {
                shootingDir = _default.shootingDir,
                shootingPos = pos
            });
        }
    }
}

