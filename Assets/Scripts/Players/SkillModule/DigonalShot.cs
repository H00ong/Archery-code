using Game.Player.Attack;
using System.Collections.Generic;
using Players.SkillModule;
using UnityEngine;

public class DiagonalShot : PlayerSkillModuleBase, IShootContributor
{
    [SerializeField, Min(0f)] private float interval = 2f;
    [SerializeField, Min(0f)] private float angle = 40f;

    private int GetBulletCount(int lvl) => Mathf.Max(2 * lvl, 2);

    public void AddBullet(List<ShotInstruction> bulletList, ShotInstruction inst)
    {
        if (bulletList == null) return;

        int count = GetBulletCount(Level);
        int pairs = count / 2;
        if (pairs <= 0) return;

        int need = bulletList.Count + count;
        if (bulletList.Capacity < need) bulletList.Capacity = need;

        Vector3 origin = inst.ShootingPos;
        Vector3 dir0 = inst.ShootingDir;
        Vector3 up     = Vector3.up;
        Vector3 right  = Vector3.Cross(up, dir0);

        for (int i = 0; i < count; i++)
        {
            int k = i - (i < pairs ? pairs : pairs - 1);
            float t = k / (float)pairs;

            float offset = t * interval;
            Vector3 pos = origin + right * offset;

            Quaternion rot = Quaternion.AngleAxis(angle * t, up);
            Vector3 dir = rot * dir0;

            bulletList.Add(new ShotInstruction
            {
                ShootingDir = dir,
                ShootingPos = pos
            });
        }
    }
}

