using System.Collections.Generic;
using UnityEngine;



public class EnemyManager : MonoBehaviour
{
    public EnemyData EnemyData { get; private set; }

    public static List<Enemy> enemies = new List<Enemy>();

    void Start()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in allEnemies) 
        {
            enemies.Add(enemy);
        }
    }

    void Update()
    {
        
    }

    public static void ChangeState(Enemy _enemy, Animator _anim, EnemyState _newState) 
    {
        _anim.SetBool(_enemy.CurrentState.ToString(), false);
        _anim.SetBool(_newState.ToString(), true);

        _enemy.CurrentState = _newState;
    }
}
