using JetBrains.Annotations;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;          // 이동 속도
    [SerializeField] private float runSpeed = 20f;          // 달리기 조정 변수
    [SerializeField] private float rotationSpeed = 10f;     // 회전 속도
    [SerializeField] private Animator animator;             // 이미 Blend Tree가 설정된 Animator
    private float currentSpeed; // 현재 이동 속도

    //점프
    public float jumpForce = 10f;
    private bool isGrounded;
    private Rigidbody rb;

    void Start()
    {
        currentSpeed = moveSpeed;                    // 기본 속도를 걷기 속도로 설정
        animator = GetComponent<Animator>();         // Animator 가져오기
        rb = GetComponent<Rigidbody>();              // 리기디바디
    }

    void Update()
    {
        // 수평, 수직 입력 받아오기 (WASD 또는 방향키)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputVector = new Vector3(horizontal, 0f, vertical);

        // 입력 벡터의 크기를 통해 Blend Tree의 Speed 파라미터에 전달할 값 계산
        // (입력 벡터의 크기가 0이면 idle, 작으면 walk, 크면 run 등으로 처리)
        float speedValue = inputVector.magnitude;

        // 입력 벡터가 0보다 크면(즉, 이동 중이면) 이동 방향 계산 및 회전
        if (speedValue > 0.1f)
        {
            // 이동 방향 (정규화된 입력 벡터)
            Vector3 moveDirection = inputVector.normalized;
            // 목표 회전(이동 방향을 바라보도록)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            // 부드럽게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            // 캐릭터 이동 (현재 forward 방향을 따라 currentSpeed 속도로 이동)
            rb.velocity = new Vector3(moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);
        }

        // Animator의 Blend Tree를 위해 "Speed" 파라미터를 업데이트
        animator.SetFloat("Velocity", speedValue * (currentSpeed / moveSpeed ));
        
        // 달리기 적용용 (Shift를 누르면 달리기 속도 적용)  
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
        }
        else // shift를 떼면 걷기 적용
        {
            currentSpeed = moveSpeed;
        }

        //점프
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (!isGrounded) // 공중에 있을 때만 추가 중력 적용
        {
            rb.velocity += Vector3.down * 10f * Time.deltaTime; // 10f 값을 더 높이면 더 빨리 떨어짐
        }
    }

    // 점프 함수 호출
    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // 기존 y 속도 초기화 (더블 점프 방지)
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // 위쪽으로 힘을 가함
        isGrounded = false; // 점프했으므로 false
    }

    // 땅에 닿아 있는지 체크
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // Ground 태그와 충돌하면
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // "Ground"에서 떨어지면
        {
            isGrounded = false; // 공중 상태
        }
    }
}