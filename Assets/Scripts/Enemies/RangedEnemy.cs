using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged Enemy Info")]
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float defaultProjectileSpeed = 10f;
    [SerializeField] float defaultProjectileLifetime = 10f; // Time after which the projectile will be destroyed if not used
    [SerializeField] int defaultRangedDamage = 1;
    [SerializeField] int defaultMeleeDamage = 1;
    Vector3 lastPlayerPosition;
    [Space]
    [Header("Ranged Follow Info")]
    [SerializeField] float defaultAttackRange = 5f;
    [Space]
    [Header("Ranged Random Info")]
    [SerializeField] float defaultPickDirectionTime = 7f;
    float pickDirectionTimer = 0f;
    [Space]
    [Header("Ranged Pattern Info")]
    [SerializeField] Transform[] patrolPoints = null;
    int currentPatrolIndex = 0;
    int patrolPointsCount = 0;

    #region Initialization

    protected override void Start()
    {
        base.Start();

        if (!debugMode)
        {
            if (enemyData != null)
            {

            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void DefaultSetting()
    {
        base.DefaultSetting();

        EnemyManager.ChangeState(this, anim, EnemyState.Idle);

        if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;

            Transform parnet = patrolPoints[0].parent;
            parnet.SetParent(null);

            patrolPointsCount = patrolPoints.Length;
        }

        RigidbodyActive(true);
    }
    #endregion

    protected override void Update()
    {
        base.Update();

        if(CurrentState == EnemyState.Dead)
            return;

        if (CurrentState == EnemyState.Idle)
            return;

        HurtEndTriggered();
        AttackEndTriggered();
        Move();
        AttackCheck();
    }

    #region Anmation Triggers
    void HurtEndTriggered()
    {
        if (hurtTriggered)
        {
            hurtTriggered = false;
            if (EnemyType != EnemyType.Ranged_Follow)
                EnemyManager.ChangeState(this, anim, EnemyState.Move);
            else
                EnemyManager.ChangeState(this, anim, EnemyState.Idle);
        }
    }

    void AttackEndTriggered()
    {
        if (attackTriggered)
        {
            attackTriggered = false;

            if (EnemyType == EnemyType.Ranged_Follow)
                EnemyManager.ChangeState(this, anim, EnemyState.Idle);
            else
                EnemyManager.ChangeState(this, anim, EnemyState.Move);
        }
    }

    #endregion

    #region Common Actions
    void Move()
    {
        if (CurrentState != EnemyState.Move)
            return;

        moveTimer -= Time.deltaTime;

        if (moveTimer < 0)
        {
            moveTimer = defaultMoveTime;

            if (EnemyType == EnemyType.Ranged_Follow)
                EnemyManager.ChangeState(this, anim, EnemyState.Idle);
            else
            {
                // 램덤 이동과 pattern 이동 enemy의 공격 상태 전환
                if (EnemyType == EnemyType.Ranged_Random)
                {
                    transform.rotation
                        = Quaternion.LookRotation(player.transform.position - transform.position);
                }

                lastPlayerPosition = player.transform.position;
                EnemyManager.ChangeState(this, anim, EnemyState.Attack);
            }

            return;
        }

        if (EnemyType == EnemyType.Ranged_Follow)
            RangedFollow_Move();
        else if (EnemyType == EnemyType.Ranged_Pattern)
            RangedPattern_Move();
        else if (EnemyType == EnemyType.Ranged_Random)
            RangedRandom_Move();
    }

    public override void GetHit(float _damage)
    {
        base.GetHit(_damage);
    }

    public override int GetAttackDamage()
    {
        return defaultMeleeDamage;
    }

    #endregion

    #region Ranged Follow Actions
    void AttackCheck()
    {
        bool enemyTypeCondition = EnemyType == EnemyType.Ranged_Follow;
        bool distanceCondition = Vector3.Distance(player.transform.position, transform.position) < defaultAttackRange;
        bool stateCondition = CurrentState == EnemyState.Move;

        if (enemyTypeCondition && distanceCondition && stateCondition)
        {
            moveTimer = defaultMoveTime;

            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
            lastPlayerPosition = player.transform.position;
            EnemyManager.ChangeState(this, anim, EnemyState.Attack);
        }
    }

    void RangedFollow_Move() 
    {
        transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.position += transform.forward * defaultMoveSpeed * Time.deltaTime;
    }

    #endregion

    #region Ranged Pattern Actions

    void RangedPattern_Move() 
    {
        if (Utils.GetXZDistance(transform.position, patrolPoints[currentPatrolIndex].position) < .5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPointsCount;
        }

        Vector3 targetPosition = patrolPoints[currentPatrolIndex].position;
        
        Vector3 direction = Utils.GetDirectionVector(targetPosition, transform.position);

        transform.position += direction * defaultMoveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    #endregion

    #region Ranged Random Actions

    void RangedRandom_Move() 
    {
        bool stateCondition = CurrentState == EnemyState.Move;
        bool enemyTypeCondition = EnemyType == EnemyType.Ranged_Random;

        if (stateCondition && enemyTypeCondition)
        {
            pickDirectionTimer -= Time.deltaTime;

            if (pickDirectionTimer <= 0f)
            {
                PickMoveDirection();
            }

            transform.position += transform.forward * defaultMoveSpeed * Time.deltaTime;
        }
    }

    private void PickMoveDirection()
    {
        pickDirectionTimer = defaultPickDirectionTime;

        Vector3 randomDir = Random.onUnitSphere;
        randomDir.y = 0; // Y축은 고정

        transform.rotation = Quaternion.LookRotation(randomDir);
    }

    private void PickReflectDirection(Vector3 _inDir, Vector3 _inNormal) 
    {
        pickDirectionTimer = defaultPickDirectionTime;

        Vector3 reflectDir = Vector3.Reflect(_inDir, _inNormal);
        reflectDir.y = 0; // Y축은 고정
        reflectDir = Quaternion.Euler(0, Random.Range(-30f, 30f), 0) * reflectDir; // 약간의 랜덤 회전 추가

        transform.rotation = Quaternion.LookRotation(reflectDir);
    }

    #endregion

    #region Animation Events
    public void Shoot(Transform _shootingPoint)
    {
        GameObject go = GameManager.Instance.StageManager.PoolManager.GetObject(projectilePrefab);

        Vector3 flyingDir = Vector3.zero;
        if (EnemyType != EnemyType.Ranged_Pattern)
            flyingDir = Utils.GetDirectionVector(lastPlayerPosition, _shootingPoint.position);
        else
            flyingDir = _shootingPoint.forward;

        Projectile_Enemy newProjectile = go.GetComponent<Projectile_Enemy>();
        newProjectile.SetupProjectile(_shootingPoint.position, flyingDir, defaultProjectileSpeed, defaultRangedDamage, defaultProjectileLifetime);
    }

    public override void Die()
    {
        base.Die();

        Destroy(gameObject, .5f);
    }
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        bool isObstacle = collision.gameObject.CompareTag("Obstacle");
        bool enemyTypeCondition = EnemyType == EnemyType.Ranged_Random;

        if (isObstacle && enemyTypeCondition)
        {
            Vector3 inDir = transform.forward;
            Vector3 inNormal = collision.contacts[0].normal;

            PickReflectDirection(inDir, inNormal);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }


    private void OnDrawGizmos()
    {
        if (EnemyType == EnemyType.Ranged_Follow) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, defaultAttackRange);
        }
    }
}

