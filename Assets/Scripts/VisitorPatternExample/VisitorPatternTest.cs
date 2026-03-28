using System.Collections.Generic;
using UnityEngine;

namespace VisitorPatternExample
{
    /// <summary>
    /// 테스트용 MonoBehaviour.
    /// 빈 GameObject에 붙이고 Play하면 Console에 결과가 출력된다.
    /// </summary>
    public class VisitorPatternTest : MonoBehaviour
    {
        private void Start()
        {
            // 1) 동물들 생성
            List<IAnimal> animals = new()
            {
                new Dog(),
                new Cat(),
                new Bird(),
            };

            // 2) Visitor(작업) 생성
            var soundVisitor = new SoundVisitor();
            var infoVisitor  = new InfoVisitor();
            var feedVisitor  = new FeedVisitor();

            // 3) 모든 동물에게 "소리" 작업 실행
            Debug.Log("========== 소리 Visitor ==========");
            // Linq
            animals.ForEach(animal => animal.Accept(soundVisitor));

            // 4) 모든 동물에게 "정보" 작업 실행
            Debug.Log("========== 정보 Visitor ==========");
            animals.ForEach(animal => animal.Accept(infoVisitor));

            // 5) 모든 동물에게 "밥주기" 작업 실행
            Debug.Log("========== 밥주기 Visitor ==========");
            animals.ForEach(animal => animal.Accept(feedVisitor));

            Debug.Log("========== 테스트 완료 ==========");
        }
    }
}
