using Game.Player;
using Players;
using Stat;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement Info")]
    [SerializeField] float sphereCastRadius = 0.5f;
    [SerializeField] LayerMask obstacleLayer;

    private PlayerStat _stat = null;
    private PlayerController _playerManager = null;

    public void ExecuteMovement(Vector2 moveInput)
    {
        _stat ??= PlayerController.Instance.Stat;
        _playerManager ??= PlayerController.Instance;

        _playerManager.ChangePlayerState(PlayerState.Move);

        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y);
        transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);

        Vector3 collisionPos = transform.position + transform.up + transform.forward * .5f;
        if (Physics.OverlapSphere(collisionPos, sphereCastRadius, obstacleLayer).Length > 0)
            return;

        transform.position += moveDir * _stat.MoveSpeed * Time.deltaTime;
    }

    #region Skill Methods

    public void UpdateMoveSpeed(float modifier)
    {
        _stat.ApplyMoveSpeedModifier(modifier);
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position + transform.up + transform.forward * .5f, sphereCastRadius);
    }
}
