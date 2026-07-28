namespace Effects
{
    /// <summary>
    /// Lightning 등 스턴류 상태이상의 대상. 
    /// 시작/종료 신호만 받고, duration 관리는 EffectHandler 쪽에서 담당한다.
    /// </summary>
    public interface IStunReceiver
    {
        void BeginStun();
        void EndStun();
    }
}
