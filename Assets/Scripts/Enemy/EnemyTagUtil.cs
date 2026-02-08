using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public static class EnemyTagUtil
    {   
        public static EnemyTag ParseTagsToMask(IEnumerable<string> tags)
        {
            if (tags == null) return EnemyTag.None;
            EnemyTag mask = EnemyTag.None;
            foreach (string tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
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

        // EnemyTag와 EffectType 매핑 테이블
        public static readonly (EnemyTag tag, EffectType effect)[] TagEffectMap = new[]
        {
            (EnemyTag.Fire, EffectType.Fire),
            (EnemyTag.Ice, EffectType.Ice),
            (EnemyTag.Poison, EffectType.Poison),
            (EnemyTag.Lightning, EffectType.Lightning),
            (EnemyTag.Magma, EffectType.Magma),
        };

        /// <summary>
        /// EnemyTag를 EffectType으로 변환
        /// </summary>
        public static EffectType ToEffectType(EnemyTag tags)
        {
            EnemyTag attributeTags = tags & EnemyTag.AttributeMask;
            EffectType result = EffectType.Normal;

            foreach (var (tag, effect) in TagEffectMap)
            {
                if (Has(attributeTags, tag))
                    result |= effect;
            }

            return result;
        }
    }

    public struct EnemyKey : IEquatable<EnemyKey>
    {
        public EnemyName Name;
        public EnemyTag Tag;

        public EnemyKey(EnemyName name, EnemyTag tag)
        {
            Name = name;
            Tag = tag;
        }

        public bool Equals(EnemyKey other)
        {
            return Name == other.Name && Tag == other.Tag;
        }

        public override bool Equals(object obj)
        {
            return obj is EnemyKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked 
            {
                return ((int)Name * 397) ^ (int)Tag;
            }
        }
        
        public static bool operator ==(EnemyKey left, EnemyKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EnemyKey left, EnemyKey right)
        {
            return !left.Equals(right);
        }

        // 디버깅용 ToString
        public override string ToString() => $"[{Name} : {Tag}]";
    };
}