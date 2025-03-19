using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;          // 이동 속도
    [SerializeField] private float runSpeed = 20f;          // 달리기 조정 변수
    [SerializeField] private float rotationSpeed = 10f;     // 회전 속도
    [SerializeField] private Animator animator;             // 이미 Blend Tree가 설정된 Animator
    private float currentSpeed; // 현재 이동 속도

    //점프
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;
    public float jumpForce = 5f;
    private bool isGrounded;
    private Rigidbody rb;

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void Start()
    {
        currentSpeed = moveSpeed;                    // 기본 속도를 걷기 속도로 설정
        animator = GetComponent<Animator>();         // Animator 가져오기
        rb = GetComponent<Rigidbody>();
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

        // 속도 결정 (Shift를 누르면 달리기 속도 적용)  
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
        }
        else // shift를 떼면 걷기 적용
        {
            currentSpeed = moveSpeed;
        }

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
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
        }

        // 이동
        Vector3 velocity = inputVector * moveSpeed * Time.deltaTime; // 이동 속도
        
        // Animator의 Blend Tree를 위해 "Speed" 파라미터를 업데이트
        animator.SetFloat("Velocity", speedValue * (currentSpeed / moveSpeed ));

        //점프
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }
}