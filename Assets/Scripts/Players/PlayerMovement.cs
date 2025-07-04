using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement Info")]
    [SerializeField] float defaultMoveSpeed = 5f;

    CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
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
        characterController.Move(new Vector3(_moveInput.x, 0, _moveInput.y).normalized * defaultMoveSpeed * Time.deltaTime);
    }
}
