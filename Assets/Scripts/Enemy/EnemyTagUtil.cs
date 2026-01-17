using System;
using System.Collections.Generic;
using Enemy;
using UnityEngine;

namespace Enemy
{
    public static class EnemyTagUtil
    {
        public static readonly EnemyTag[] AllTags = (EnemyTag[])System.Enum.GetValues(typeof(EnemyTag));
        
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