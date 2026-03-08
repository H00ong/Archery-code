using System.Collections.Generic;
using System.Linq; // ★ LINQ 필수!
using UnityEngine; // ★ Unity 필수

public class Test : MonoBehaviour
{
    public enum MoveMode 
    { 
        TransformPosition, 
        RbVelocity, 
        RbMovePosition 
    }

    [Header("현재 이동 방식 (1, 2, 3 키로 변경)")]
    public MoveMode currentMode = MoveMode.TransformPosition;
    
    [Header("이동 속도")]
    public float speed = 5f;

    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 회전이나 넘어짐 방지를 위해 x, z축 회전 고정
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 1. 입력 받기 (WASD 또는 방향키)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(x, 0, z).normalized;

        // 2. 숫자 키로 이동 모드 실시간 변경
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeMode(MoveMode.TransformPosition);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeMode(MoveMode.RbVelocity);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeMode(MoveMode.RbMovePosition);

        // [방식 1] transform.position (물리 무시, 텔레포트 방식)
        // Update에서 Time.deltaTime을 곱해서 실행해야 부드럽습니다.
        if (currentMode == MoveMode.TransformPosition)
        {
            transform.position += moveInput * speed * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // [방식 2] rb.velocity (물리 엔진 속도 제어)
        if (currentMode == MoveMode.RbVelocity)
        {
            // 원하는 속도를 직접 대입
            rb.linearVelocity = moveInput * speed;
        }
        
        // [방식 3] rb.MovePosition (물리 엔진 위치 이동)
        else if (currentMode == MoveMode.RbMovePosition)
        {
            // 현재 위치에서 입력 방향으로 고정 프레임 시간만큼 이동
            Vector3 targetPos = rb.position + moveInput * speed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);
        }
    }

    // 모드 변경 시 로그 출력 및 초기화
    private void ChangeMode(MoveMode newMode)
    {
        currentMode = newMode;
        Debug.Log("현재 이동 모드: " + currentMode.ToString());
        
        // 다른 모드로 변경할 때 남아있는 물리 속력을 0으로 초기화 (관성 방지)
        rb.linearVelocity = Vector3.zero;
    }
}
