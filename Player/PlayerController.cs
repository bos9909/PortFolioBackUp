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
    // ★ 추가: 마녀가 위아래로 뒤집히지 않도록 제한할 최대 각도
    [SerializeField] private float maxPitchAngle = 50f;
    
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab; // 발사할 투사체 프리팹 (나중에 에디터에서 할당)
    [SerializeField] private Transform firePoint;     // 총알이 나갈 빗자루 끝부분 위치
    [SerializeField] private float fireRate = 0.2f;    // 연사 속도 (초 단위)

    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector3 moveDirection;
    private Vector3 targetVelocity;
    
    private float nextFireTime;
    private Camera mainCamera;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        mainCamera = Camera.main; // 메인 카메라 캐싱
        Debug.Log("[PlayerController] 컴포넌트 초기화 완료 (CharacterController, PlayerInput, MainCamera)");
        
        // 호버링 비행이므로 유니티 자체 일반 중력은 끕니다. (우리가 마법 중력을 제어)
        rb.useGravity = false;
        
        // 회전력을 물리 엔진이 강제로 리셋하지 못하게 고정합니다.
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        Debug.Log("[PlayerController] 마녀 빗자루 호버링 시스템 초기화 완료 (중력 비활성화, Rigidbody 세팅)");
    }

    void FixedUpdate()
    {
        HoverMove();
        LookAtMouse3D();
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
    
    private void LookAtMouse3D()
    {
        Ray ray = mainCamera.ScreenPointToRay(playerInput.MousePosition);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(maxAimDistance);
        }

        // 1. 조준점을 향한 방향 벡터 계산
        Vector3 lookDirection = (targetPoint - transform.position).normalized;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            // 2. 목표 회전값 계산
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            Vector3 targetEuler = targetRotation.eulerAngles;

            // 3. ★ 핵심: 위아래 꺾임 각도(X축 회전) 제한 알고리즘
            // 유니티 퀴터니언 오일러각(0~360도)을 앙각(-180~180도)으로 보정합니다.
            float pitch = targetEuler.x;
            if (pitch > 180f) pitch -= 360f;

            // 인스펙터에서 정한 maxPitchAngle 이내로 위아래 각도를 가둡니다.
            pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);

            // 4. 제한된 각도를 적용한 최종 회전값 조립
            Quaternion clampedRotation = Quaternion.Euler(pitch, targetEuler.y, 0f); // Roll(Z축)은 0으로 고정하여 회전 시 뒤틀림 방지
            
            // 5. 부드러운 회전 반영
            transform.rotation = Quaternion.Slerp(transform.rotation, clampedRotation, rotateSpeed * Time.fixedDeltaTime);
        }

        // 씬 창에서 확인할 수 있는 조준 가이드선
        Debug.DrawLine(firePoint ? firePoint.position : transform.position, targetPoint, Color.red);
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
        Debug.Log("[PlayerController] 마법 탄환 발사!");
        
        // 효과음 연동 (SoundManager 세팅해두신 것 활용)
        if (SoundManager.Instance != null)
        {
            // SoundManager.Instance.PlaySFX("MagicShot"); 
        }

        // 프리팹과 발사 지점이 등록되어 있다면 실제로 생성합니다.
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
    
}
