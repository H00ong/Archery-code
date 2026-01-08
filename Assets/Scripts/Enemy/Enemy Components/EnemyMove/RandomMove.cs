using Enemies;
using Enemy;
using UnityEngine;

[System.Serializable]
public class RandomMove : EnemyMove
{
    [SerializeField] float pickDirectionTime = 7f;
    float pickDirectionTimer;

    public override void Init(EnemyController c)
    {
        base.Init(c);

        pickDirectionTimer = pickDirectionTime;
    }
        
    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();

        _ctx.lastPlayerPosition = _player.transform.position;

        Vector3 dir = Utils.GetXZDirectionVector(_ctx.lastPlayerPosition, transform.position);
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public override void Tick()
    {
        UpdateState(EnemyState.Attack);
    }

    protected override void UpdateState(EnemyState state)
    {
        _moveTimer -= Time.deltaTime;
        pickDirectionTimer -= Time.deltaTime;

        if (_moveTimer < 0)
        {
            _moveTimer = defaultMoveTime;
            _ctx.ChangeState(state);
            return;
        }

        if (pickDirectionTimer < 0)
        {
            PickMoveDirection();
        }

        ForwardMove();
    }

    private void PickMoveDirection()
    {
        pickDirectionTimer = pickDirectionTime;

        Vector3 randomDir = UnityEngine.Random.onUnitSphere;
        randomDir.y = 0;

        transform.rotation = Quaternion.LookRotation(randomDir);
    }

    public void PickReflectDirection(Vector3 _inDir, Vector3 _inNormal)
    {
        pickDirectionTimer = pickDirectionTime;

        Vector3 reflectDir = Vector3.Reflect(_inDir, _inNormal);
        reflectDir.y = 0;
        reflectDir = Quaternion.Euler(0, UnityEngine.Random.Range(-30f, 30f), 0) * reflectDir;

        transform.rotation = Quaternion.LookRotation(reflectDir);
    }
}
