namespace VisitorPatternExample
{
    /// <summary>
    /// "방문자" 인터페이스.
    /// 동물 종류마다 다르게 행동하고 싶은 "작업"을 여기에 정의한다.
    /// </summary>
    public interface IAnimalVisitor
    {
        void Visit(Dog dog);
        void Visit(Cat cat);
        void Visit(Bird bird);
    }
}
