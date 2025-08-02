using System.Collections.Generic;
using UnityEngine;



public class EnemyManager : MonoBehaviour
{
    public static List<Enemy> enemies = new List<Enemy>();

    void Start()
    {
        FindAllEnemies();
    }

    public static void FindAllEnemies()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in allEnemies)
        {
            enemies.Add(enemy);
        }
    }

    public static void ClearAllEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        enemies.Clear();
    }

    public static void ChangeState(Enemy _enemy, Animator _anim, EnemyState _newState) 
    {
        _anim.SetBool(_enemy.CurrentState.ToString(), false);
        _anim.SetBool(_newState.ToString(), true);

        _enemy.CurrentState = _newState;
    }
}
