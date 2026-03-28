using UnityEngine;

namespace VisitorPatternExample
{
    /// <summary> 강아지 </summary>
    public class Dog : IAnimal
    {
        public string Name = "멍멍이";
        public int Loyalty = 100;

        // 핵심: 자기 자신(this)을 visitor에게 넘긴다
        public void Accept(IAnimalVisitor visitor) => visitor.Visit(this);
    }

    /// <summary> 고양이 </summary>
    public class Cat : IAnimal
    {
        public string Name = "냥이";
        public int Independence = 80;

        public void Accept(IAnimalVisitor visitor) => visitor.Visit(this);
    }

    /// <summary> 새 </summary>
    public class Bird : IAnimal
    {
        public string Name = "짹짹이";
        public float WingSpan = 1.2f;

        public void Accept(IAnimalVisitor visitor) => visitor.Visit(this);
    }
}
