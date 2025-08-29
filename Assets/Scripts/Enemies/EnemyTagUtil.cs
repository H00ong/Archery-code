using System.Collections.Generic;
using System;
using UnityEngine;
using Game.Enemies.Enum;

namespace Game.Enemies
{
    public static class EnemyTagUtil
    {
        public static EnemyTag ParseTagsToMask(IEnumerable<string> tags)
        {
            if (tags == null) return EnemyTag.None;
            EnemyTag mask = EnemyTag.None;
            foreach (string tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue; // 빈 문자나 공백문자의 경우를 위한 예외처리
                var norm = tag.Trim();
                if (System.Enum.TryParse(norm, ignoreCase: true, out EnemyTag t))
                    mask |= t;
                else
                    Debug.LogWarning($"Unknown tag: {tag}");
            }

            return mask;
        }

        public static EnemyTag ParseTagsToMask(string tag)
        {
            return ParseTagsToMask(new[] { tag });
        }

        public static bool Has(EnemyTag mask, EnemyTag t) => (mask & t) != 0;
    }

    public struct EnemyKey : IEquatable<EnemyKey>
    {
        public EnemyName Name;
        public EnemyTag Tag;

        public bool Equals(EnemyKey other) => Name == other.Name && Tag == other.Tag;
        public override int GetHashCode() => ((int)Name * 397) ^ (int)Tag; // 해시테이블은 해시 값이 필요하므로 이를 위해서 397이라는 임의의 소수를 사용
    };

    
}