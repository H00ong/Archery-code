using Enemy;
using UnityEngine;

[System.Serializable]
public class RandomMove : EnemyMove
{
    private float _pickDirectionTimer;
    private float _pickDirectionTime;

    public override void Init(EnemyController ctx, BaseModuleData data = null)
    {
        base.Init(ctx, data);

        if (data is RandomMoveData randomData)
        {
            _pickDirectionTime = randomData.pickDirectionTime;
        }
        else
        {
            _pickDirectionTime = 7f;
        }

        _pickDirectionTimer = _pickDirectionTime;
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
        _moveTimer -= Time.deltaTime;
        _pickDirectionTimer -= Time.deltaTime;

        if (_moveTimer < 0)
        {
            _moveTimer = _duration;
            _ctx.OnModuleComplete();
            return;
        }

        if (_pickDirectionTimer < 0)
        {
            PickMoveDirection();
        }

        MoveForward();
    }
    
    private void PickMoveDirection()
    {
        _pickDirectionTimer = _pickDirectionTime;

        Vector3 randomDir = UnityEngine.Random.onUnitSphere;
        randomDir.y = 0;

        transform.rotation = Quaternion.LookRotation(randomDir);
    }

    public void PickReflectDirection(Vector3 inDir, Vector3 inNormal)
    {
        _pickDirectionTimer = _pickDirectionTime;

        Vector3 reflectDir = Vector3.Reflect(inDir, inNormal);
        reflectDir.y = 0;
        reflectDir = Quaternion.Euler(0, UnityEngine.Random.Range(-30f, 30f), 0) * reflectDir;

        transform.rotation = Quaternion.LookRotation(reflectDir);
    }
}
