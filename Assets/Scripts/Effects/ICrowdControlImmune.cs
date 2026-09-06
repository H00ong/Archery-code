namespace Effects
{
    /// <summary>
    /// 특정 EffectType(상태이상)에 대해 완전히 면역인 대상. (예: Boss의 Ice/Lightning 면역)
    /// Health가 이 컴포넌트를 발견하면 해당 EffectType의 핸들러를 아예 적용하지 않는다.
    /// </summary>
    public interface ICrowdControlImmune
    {
        bool IsImmuneTo(EffectType type);
    }
}
