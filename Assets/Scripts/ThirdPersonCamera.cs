using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target & Base Offset")]
    [SerializeField] private Transform target;                         
    [SerializeField] private Vector3 offset = new Vector3(-3f, 5f, 0f);

    [Header("Movement Settings")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Occlusion / Raise Settings")]
    [SerializeField] private float headYOffset = 10f;                     
    [SerializeField] private float occlusionHoldTime = 0.5f;              

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallMask;     // 벽 감지용 레이어

    // ---- 새로 추가된 부분: 복도 트리거 상태를 외부에서 변경하기 위해 public
    [HideInInspector] public bool inCorridor = false;

    private float occlusionTimer = 0f;
    private float fixedYaw;

    void Start()
    {
        fixedYaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 기본 카메라 위치
        Vector3 defaultPosition = target.position + offset;

        // 선형 Raycast로 벽 감지
        bool wallOccluded = CheckWallOcclusion(target.position, defaultPosition);

        // 만약 벽이 감지되거나, 캐릭터가 복도 안에 있다면 카메라를 머리 위로
        if (wallOccluded || inCorridor)
        {
            occlusionTimer = occlusionHoldTime;
        }
        else
        {
            occlusionTimer = Mathf.Max(0f, occlusionTimer - Time.deltaTime);
        }

        bool shouldRaise = occlusionTimer > 0f;

        // shouldRaise가 true면 캐릭터 머리 위로 이동, 아니면 기본 오프셋
        Vector3 desiredPosition = shouldRaise
            ? target.position + new Vector3(0, headYOffset, 0)
            : defaultPosition;

        // 부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 카메라 회전은 y축(수평)을 고정
        Vector3 lookDir = target.position - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            Vector3 euler = targetRotation.eulerAngles;
            euler.y = fixedYaw;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), smoothSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 캐릭터~카메라 사이 선형 Raycast로 벽이 있는지 감지
    /// </summary>
    private bool CheckWallOcclusion(Vector3 from, Vector3 to)
    {
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);

        RaycastHit hit;
        if (Physics.Raycast(from, dir, out hit, dist, wallMask))
        {
            if (hit.collider.gameObject != target.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
