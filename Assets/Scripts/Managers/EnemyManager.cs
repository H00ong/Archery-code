using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public EnemyData EnemyData { get; private set; }

    public static List<Enemy> enemies = new List<Enemy>();

    void Start()
    {
        Enemy TestEnemy = GameObject.Find("TestEnemy").GetComponent<Enemy>();
        enemies.Add(TestEnemy);
    }

    void Update()
    {
        
    }

    public void GenerateEnemy() 
    {

    }
}
