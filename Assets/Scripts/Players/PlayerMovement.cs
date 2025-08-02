using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement Info")]
    [SerializeField] float defaultMoveSpeed = 5f;
    [SerializeField] float sphereCastRadius = 0.5f;
    [SerializeField] LayerMask obstacleLayer;

    private void Awake()
    {
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void Move(Vector2 _moveInput)
    {
        GameManager.Instance.PlayerManager.ChangePlayerState(PlayerState.Move);

        Vector3 moveDir = new Vector3(_moveInput.x, 0, _moveInput.y);
        transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

        // º® ºÎµúÈû °Ë»ç
        if (Physics.OverlapSphere(transform.position + transform.up + transform.forward * 1 / 2, sphereCastRadius, obstacleLayer).Length > 0)
            return;

        transform.position += moveDir * defaultMoveSpeed * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward * 1 / 2, sphereCastRadius);
    }
}
