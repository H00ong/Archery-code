using System.Collections.Generic;
using Game.Player.Attack;
using Players.SkillModule;
using UnityEngine;
using static UnityEditorInternal.ReorderableList;

public class HorizontalShot : PlayerSkillModuleBase, IShootContributor
{
    [SerializeField, Min(0f)]
    private float interval = 1f;

    private int GetBulletCount(int lvl)
    {
        return lvl + 1;
    }

    public void AddBullet(List<ShotInstruction> bulletList, ShotInstruction inst)
    {
        if (bulletList == null)
            return;

        int count = GetBulletCount(Level);
        if (count <= 0)
            return;

        int need = bulletList.Count + count;
        if (bulletList.Capacity < need)
            bulletList.Capacity = need;

        Vector3 origin = inst.ShootingPos;
        Vector3 dir = inst.ShootingDir;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, dir);

        float half = (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offset = (i - half) * interval;
            Vector3 pos = origin + right * offset;

            bulletList.Add(
                new ShotInstruction { ShootingDir = inst.ShootingDir, ShootingPos = pos }
            );
        }
    }
}
