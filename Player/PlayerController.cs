using System;
using UnityEngine;

//필요한 컴포넌트
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerController : MonoBehaviour
{
    [Header("Hovering Movement Settings")]
    [SerializeField] private float moveSpeed = 12f;       // 비행 최고 속도
    [SerializeField] private float acceleration = 5f;     // 비행 가속도 (높을수록 빠르게 최고 속도 도달)
    [SerializeField] private float busterDamping = 3f;    // 감속 제동력 (키를 뗐을 때 멈추는 부드러움 수준)

    [Header("3D Aiming Settings")]
    [SerializeField] private float rotateSpeed = 15f;
    [SerializeField] private float maxAimDistance = 50f;
    // 마녀가 위아래로 뒤집히지 않도록 제한할 최대 각도
    [SerializeField] private float maxPitchAngle = 50f;
    // 전방 기준 좌우로 최대 몇 도까지 꺾일 수 있는지 설정 (90도 설정 시 전방 180도 커버)
    [SerializeField] private float maxYawAngle = 90f;
    // 플레이어 흔들림 제거를 위한 레이어 마스크
    [SerializeField] private LayerMask aimTargetLayer = ~0;
    
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab; // 발사할 투사체 프리팹 (나중에 에디터에서 할당)
    [SerializeField] private GameObject muzzleFlashPrefab; // 총구 화염 프리팹
    [SerializeField] private Transform firePoint;     // 총알이 나갈 빗자루 끝부분 위치
    [SerializeField] private float fireRate = 0.2f;    // 연사 속도 (초 단위)

    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector3 moveDirection;
    private Vector3 targetVelocity;
    
    private float nextFireTime;
    private Camera mainCamera;
    
    //패드 조준용
    public Vector3 CurrentAimPoint { get; private set; }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main; // 메인 카메라 캐싱
        Debug.Log("[PlayerController] 컴포넌트 초기화 완료 (CharacterController, PlayerInput, MainCamera)");
        
        // 호버링 비행이므로 유니티 자체 일반 중력은 끕니다. (중력을 제어)
        rb.useGravity = false;
        
        // 회전력을 물리 엔진이 강제로 리셋하지 못하게 고정
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        Debug.Log("[PlayerController] 마녀 빗자루 호버링 시스템 초기화 완료 (중력 비활성화, Rigidbody 세팅)");
    }

    void FixedUpdate()
    {
        HoverMove();
        // ★ 하이브리드 조준 시스템 작동
        if (playerInput.IsUsingGamepad)
        {
            LookAtGamepad3D();
        }
        else
        {
            LookAtMouse3D();
        }
    }

    private void Update()
    {
        // 사격은 물리가 아니므로 일반 Update에서 처리합니다.
        TryShoot();
    }

    private void HoverMove()
    {
        //방향 및 타겟 속도 계산
        Vector3 inputDirection = new Vector3(playerInput.MoveX, playerInput.MoveY, playerInput.MoveZ).normalized;
        Vector3 targetVelocity = inputDirection * moveSpeed;

        //선형 속도로 보간
        float currentSpeed = inputDirection.sqrMagnitude > 0.001f ? acceleration : busterDamping;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, currentSpeed * Time.fixedDeltaTime);
    }
    

    // 마우스 조준 보정
    private void LookAtMouse3D()
    {
        Ray ray = mainCamera.ScreenPointToRay(playerInput.MousePosition);
        Vector3 targetPoint;

        // ★ 수정: Raycast 맨 마지막 인자에 aimTargetLayer를 추가합니다.
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimTargetLayer)) 
        {
            targetPoint = hit.point;
        }
        else 
        {
            targetPoint = ray.GetPoint(maxAimDistance);
        }
        
        Vector3 lookDirection = (targetPoint - transform.position).normalized;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            // ★ 핵심: 전방 180도 제한 쿼터니언 연산 적용
            Quaternion clampedRotation = GetClampedAimRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, clampedRotation, rotateSpeed * Time.fixedDeltaTime);
        }

        CurrentAimPoint = targetPoint;
        Debug.DrawLine(firePoint != null ? firePoint.position : transform.position, CurrentAimPoint, Color.red);
    }
    
    //게임 패드용 조준
   
    private void LookAtGamepad3D()
    {
        Vector2 stickInput = playerInput.PadLookVector;

        if (stickInput.sqrMagnitude > 0.05f) // 데드존 설정
        {
            // 1. 기준이 되는 카메라의 수평 정면 방향 구하기
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            // 카메라 기준 정면의 기본 Y축 회전각(Yaw)
            float baseYaw = Quaternion.LookRotation(cameraForward).eulerAngles.y;

            // 2. [좌우 회전 제한] 스틱의 좌우(X) 입력만큼 각도 계산 (-90도 ~ +90도)
            float yawOffset = stickInput.x * maxYawAngle; 
            float finalYaw = baseYaw + yawOffset;

            // 3. [위아래 회전 제한] 스틱의 위아래(Y) 입력만큼 각도 계산 (-maxPitchAngle ~ +maxPitchAngle)
            // 스틱을 위로 밀면(Y+) 하늘을 봐야 하므로 음수(-) 방향 앙각을 줍니다 (유니티 X축 회전 특성)
            float finalPitch = -stickInput.y * maxPitchAngle;

            // 4. 독립된 두 각도를 하나의 깨끗한 쿼터니언으로 조립 (Roll은 0 고정)
            // 이렇게 축을 쪼개서 조립하면 절대 좌우로 발작하듯 휙휙 돌아가지 않습니다.
            Quaternion targetRotation = Quaternion.Euler(finalPitch, finalYaw, 0f);

            // 5. 부드럽게 마녀 회전 적용
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);

            // 6. 카메라가 쫓아올 3D 조준점도 내 앞방향으로 갱신
            CurrentAimPoint = transform.position + (transform.forward * (maxAimDistance * 0.5f));
        }
        else
        {
            // 스틱을 놓으면 정면 복귀
            CurrentAimPoint = transform.position + (transform.forward * 2f);
        }

        Debug.DrawLine(firePoint != null ? firePoint.position : transform.position, CurrentAimPoint, Color.red);
    }
    
    // ★ 추가: 마우스/패드 공용 180도 조준 각도 제한 헬퍼 함수
    private Quaternion GetClampedAimRotation(Vector3 targetDirection)
    {
        // 1. 입력된 조준 방향을 쿼터니언 오일러로 변환
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Vector3 targetEuler = targetRotation.eulerAngles;

        // 2. 위아래(Pitch) 제한
        float pitch = targetEuler.x;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);

        // 3. 좌우(Yaw) 제한 연산
        // 현재 마녀의 휭 이동이나 월드 기준이 아니라, 
        // "카메라가 바라보는 정면 방향"을 기준으로 삼아야 숄더뷰의 전방 180도가 유지됩니다.
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0f; // 수평 기준 정면
        cameraForward.Normalize();

        // 카메라 정면과 입력된 조준 방향 사이의 좌우 사잇각 구하기
        float yawOffset = Vector3.SignedAngle(cameraForward, targetDirection, Vector3.up);

        // 사잇각을 설정된 범위(예: -90도 ~ +90도)로 제한
        yawOffset = Mathf.Clamp(yawOffset, -maxYawAngle, maxYawAngle);

        // 카메라 기준 정면 각도에 제한된 사잇각을 더해 최종 수평 회전각 산출
        float finalYaw = Quaternion.LookRotation(cameraForward).eulerAngles.y + yawOffset;

        // 4. 최종 조립
        return Quaternion.Euler(pitch, finalYaw, 0f);
    }
    

    private void TryShoot()
    {
        // 입력 시스템에서 발사 키를 누르고 있고, 연사 쿨타임이 지났는지 확인
        if (playerInput.IsFiring && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (!firePoint || !ObjectPoolManager.Instance|| !bulletPrefab) return;

        // 1. 총알이 날아가야 할 최종 방향 벡터를 구합니다. (목적지 - 출발지)
        Vector3 fireDirection = (CurrentAimPoint - firePoint.position).normalized;

        if (fireDirection.sqrMagnitude > 0.001f)
        {
            // 2. 해당 방향을 똑바로 바라보는 깨끗한 쿼터니언 회전값을 산출합니다.
            // 마녀의 몸체 회전이 임계점에서 부러지더라도, 이 연산은 점과 점 사이의 방향이라 절대 부러지지 않습니다.
            Quaternion bulletRotation = Quaternion.LookRotation(fireDirection);

            // 3. 오브젝트 풀에서 총알 총구 화염을 꺼내며 정확한 회전값을 부여합니다.
            ObjectPoolManager.Instance.Pop(muzzleFlashPrefab, firePoint.position, bulletRotation);
            ObjectPoolManager.Instance.Pop(bulletPrefab, firePoint.position, bulletRotation);
        
            //디버그용 로그
            Debug.Log("[Shoot] 조준점을 향해 탄환 발사");
        }
    }
    
}
