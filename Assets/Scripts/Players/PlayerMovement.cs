using Game.Player;
using Players;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Required Managers")]
    [SerializeField] PlayerController playerManager;
    [Header("Player Movement Info")]
    [SerializeField] float defaultMoveSpeed = 5f;
    [SerializeField] float sphereCastRadius = 0.5f;
    [SerializeField] LayerMask obstacleLayer;
    public float Movespeed { get; set; }

    private void Start()
    {
        Init();
    }

    private void Init() 
    {
        Movespeed = defaultMoveSpeed;        
    }


    public void Move(Vector2 _moveInput)
    {
        playerManager.ChangePlayerState(PlayerState.Move);

        Vector3 moveDir = new Vector3(_moveInput.x, 0, _moveInput.y);
        transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

        Vector3 collisionPos = transform.position + transform.up + transform.forward * .5f;
        if (Physics.OverlapSphere(collisionPos, sphereCastRadius, obstacleLayer).Length > 0)
            return;

        transform.position += moveDir * Movespeed * Time.deltaTime;
    }

    #region Skill Methods

    public void UpdateMoveSpeed(float _modifier) => Movespeed = defaultMoveSpeed * (1 + _modifier);

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward * .5f, sphereCastRadius);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerManager == null) playerManager = GetComponent<PlayerController>();
    }
#endif
}
