using UnityEngine;

namespace VisitorPatternExample
{
    /// <summary>
    /// Visitor 1: 동물 소리를 출력하는 작업
    /// → 동물 클래스 수정 없이 "새로운 행동"을 추가한 것!
    /// </summary>
    public class SoundVisitor : IAnimalVisitor
    {
        public void Visit(Dog dog)
        {
            Debug.Log($"[소리] {dog.Name}: 멍멍! 왈왈!");
        }

        public void Visit(Cat cat)
        {
            Debug.Log($"[소리] {cat.Name}: 야옹~ 그르릉~");
        }

        public void Visit(Bird bird)
        {
            Debug.Log($"[소리] {bird.Name}: 짹짹! 삐약!");
        }
    }

    /// <summary>
    /// Visitor 2: 동물 정보를 출력하는 작업
    /// → 또 다른 새로운 행동을 동물 클래스 수정 없이 추가!
    /// </summary>
    public class InfoVisitor : IAnimalVisitor
    {
        public void Visit(Dog dog)
        {
            Debug.Log($"[정보] {dog.Name} — 충성도: {dog.Loyalty}");
        }

        public void Visit(Cat cat)
        {
            Debug.Log($"[정보] {cat.Name} — 독립심: {cat.Independence}");
        }

        public void Visit(Bird bird)
        {
            Debug.Log($"[정보] {bird.Name} — 날개 폭: {bird.WingSpan}m");
        }
    }

    /// <summary>
    /// Visitor 3: 동물에게 밥을 주는 작업
    /// → 이렇게 Visitor만 새로 만들면 기존 코드 건드리지 않고 기능 확장 가능
    /// </summary>
    public class FeedVisitor : IAnimalVisitor
    {
        public void Visit(Dog dog)
        {
            Debug.Log($"[밥] {dog.Name}에게 사료를 줬다! 꼬리 흔들흔들~");
        }

        public void Visit(Cat cat)
        {
            Debug.Log($"[밥] {cat.Name}에게 참치캔을 줬다! 그르릉~");
        }

        public void Visit(Bird bird)
        {
            Debug.Log($"[밥] {bird.Name}에게 모이를 줬다! 쪼아쪼아~");
        }
    }
}
