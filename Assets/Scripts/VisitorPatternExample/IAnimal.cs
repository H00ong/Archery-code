namespace VisitorPatternExample
{
    /// <summary>
    /// "방문 받는 쪽" 인터페이스.
    /// 모든 동물은 "나한테 방문자가 왔다" → Accept로 받아준다.
    /// </summary>
    public interface IAnimal
    {
        void Accept(IAnimalVisitor visitor);
    }
}
