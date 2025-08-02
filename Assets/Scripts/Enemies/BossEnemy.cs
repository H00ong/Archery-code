using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossEnemy : Enemy
{
    [Header("Boss Enemy Projectile Info")]
    [SerializeField] GameObject flyingProjectile;
    [SerializeField] GameObject shootingProjectile;
    [SerializeField] float defaultFlyingProjectile = 10f;
    [SerializeField] float defaultProjectileSpeed = 8f;
    [SerializeField] float defaultProjectileLifetime = 10f; // Time after which the projectile will be destroyed if not used
    [Space]
    [Header("Boss Enemy Attack Info")]
    [SerializeField] int defaultCollideDmaage = 3;
    [SerializeField] int defaultMainDamage = 5;
    [SerializeField] int defaultSubDamage = 3;
    [SerializeField] float defaultAttackRange = 5f;
    Vector3 lastPlayerPosition;
    int currentAttackIndex = 0;
    int currentMoveIndex = 0;
    bool isTargeting = false;
    [Space]
    [Header("Boss Enemy Move Info")]
    [SerializeField] float defaultPickDirectionTime = 7f;
    [SerializeField] float defaultRunSpeed = 8;
    float pickDirectionTimer = 0f;

    

    #region Initialization
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void DefaultSetting()
    {
        base.DefaultSetting();

        EnemyManager.ChangeState(this, anim, EnemyState.Idle);

        RigidbodyActive(true);
    }
    #endregion

    protected override void Update()
    {
        if (CurrentState == EnemyState.Dead)
            return;

        if (CurrentState == EnemyState.Idle)
        {
            idleTimer -= Time.deltaTime;

            if (idleTimer < 0)
            {
                idleTimer = defaultIdleTime;

                EnemyManager.ChangeState(this, anim, EnemyState.Move);

                return;
            }
        }

        AttackEndTriggered();
        Move();
    }

    #region Animation Triggers
    void AttackEndTriggered()
    {
        if (attackTriggered)
        {
            attackTriggered = false;

            EnemyManager.ChangeState(this, anim, EnemyState.Idle);
        }
    }
    #endregion

    #region 

    public override int GetAttackDamage()
    {
        base.GetAttackDamage();

        return defaultCollideDmaage;
    }

    public override void GetHit(float _damage)
    {
        if (CurrentState == EnemyState.Dead)
            return;

        enemyHealth.TakeDamage(_damage);

        if (enemyHealth.IsDead())
        {
            ColliderActive(false); // Disable collider on death
            RigidbodyActive(false); // Disable rigidbody on death
            EnemyManager.enemies.Remove(this);
            EnemyManager.ChangeState(this, anim, EnemyState.Dead);
        }
        else
        {
            // visual effects
        }
    }

    #endregion

    #region Move Methods
    private void Move()
    {
        if (CurrentState != EnemyState.Move)
            return;

        moveTimer -= Time.deltaTime;

        if (moveTimer <= 0 && !isTargeting) 
        {
            moveTimer = defaultMoveTime;

            lastPlayerPosition = player.transform.position;
            transform.rotation = Quaternion.LookRotation(lastPlayerPosition - transform.position);

            currentAttackIndex = Random.Range(0, 2);
            anim.SetFloat("AttackIndex", currentAttackIndex);

            if (EnemyType == EnemyType.Boss_Ranged) 
            {
                EnemyManager.ChangeState(this, anim, EnemyState.Attack);
            }
            else if (EnemyType == EnemyType.Boss_Follow )
            {
                if (currentAttackIndex == 1)
                {
                    isTargeting = true;
                    anim.SetFloat("MoveIndex", 1);
                }
                else 
                {
                    EnemyManager.ChangeState(this, anim, EnemyState.Attack);
                }
            }
                
        }

        if (CurrentState != EnemyState.Move)
            return;

        if (!isTargeting)
            RandomMove();
        else
            MoveTowardPlayer();
    }

    private void MoveTowardPlayer() 
    {
        if (AttackCheck())
        {
            isTargeting = false;
            anim.SetFloat("MoveIndex", 0);

            lastPlayerPosition = player.transform.position;
            transform.rotation = Quaternion.LookRotation(Utils.GetDirectionVector(player.transform, transform));

            EnemyManager.ChangeState(this, anim, EnemyState.Attack);
        }
        else 
        { 
            transform.rotation = Quaternion.LookRotation(Utils.GetDirectionVector(player.transform, transform));
            transform.position += transform.forward * defaultRunSpeed * Time.deltaTime;
        }
    }


    private void RandomMove()
    {
        pickDirectionTimer -= Time.deltaTime;

        if (pickDirectionTimer <= 0f)
        {
            PickMoveDirection();
        }

        transform.position += transform.forward * defaultMoveSpeed * Time.deltaTime;
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

    private bool AttackCheck() 
    {
        bool distanceCondition = Vector3.Distance(player.transform.position, transform.position) < defaultAttackRange;
        bool stateCondition = CurrentState == EnemyState.Move;
        
        return distanceCondition && stateCondition;
    }

    #region Animation Events

    public void FlyingTargetingShoot(Transform _shootingPoint)
    {
        GameObject go = GameManager.Instance.StageManager.PoolManager.GetObject(flyingProjectile);

        Vector3 flyingDir = Vector3.zero;

        float distance = Utils.GetXZDistance(_shootingPoint.position, lastPlayerPosition);
        float flyTime = distance / defaultFlyingProjectile;
        float yVelocity = -Physics.gravity.y * flyTime / 2f - _shootingPoint.position.y / flyTime ; // Calculate the vertical velocity needed to reach the player

        flyingDir = Utils.GetDirectionVector(lastPlayerPosition, _shootingPoint.position);
        flyingDir = flyingDir * defaultFlyingProjectile + Vector3.up * yVelocity;

        Projectile_Boss newProjectile = go.GetComponent<Projectile_Boss>();
        newProjectile.SetupProjectile(_shootingPoint.position, flyingDir, -1f, defaultMainDamage, defaultProjectileLifetime);
    }

    public void Shoot(Transform _shootingPoint)
    {
        GameObject go = GameManager.Instance.StageManager.PoolManager.GetObject(shootingProjectile);

        Vector3 flyingDir = _shootingPoint.forward;

        Projectile_Enemy newProjectile = go.GetComponent<Projectile_Enemy>();
        newProjectile.SetupProjectile(_shootingPoint.position, flyingDir, defaultProjectileSpeed, defaultSubDamage, defaultProjectileLifetime);
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
        bool enemyStateCondition = CurrentState == EnemyState.Move;

        if (isObstacle && enemyStateCondition)
        {
            Vector3 inDir = transform.forward;
            Vector3 inNormal = collision.contacts[0].normal;

            PickReflectDirection(inDir, inNormal);
        }
    }

    private void OnDrawGizmos()
    {
        if (EnemyType == EnemyType.Boss_Follow) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, defaultAttackRange);
        }
    }
}
