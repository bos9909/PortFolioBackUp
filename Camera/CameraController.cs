using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    // 따라다닐 플레이어
    [SerializeField] private Transform target;
    
    [Header("Position Offset (Distance)")]
    // 플레이어 기준으로 카메라가 위치할 기본 거리 (X, Y, Z)
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 5f, -7f); 

    [Header("Rotation Settings (Angles)")]
    // 플레이어를 내려다볼 각도 (Eular Angles 사용)
    // X축: 위아래 각도 (30~45도 기본값)
    // Y축: 플레이어 등 뒤 기준 좌우 각도 (0도 기본값)
    [SerializeField] private Vector3 rotationOffset = new Vector3(30f, 0f, 0f);
    
    [Header("Mouse Lead Settings (동적 시야)")]
    // ★ 핵심: 카메라가 마우스를 얼마나 따라갈지 결정하는 비율 (0 ~ 1)
    // 0이면 마녀만 고정해서 보고, 0.5면 마녀와 마우스의 딱 중간을 봅니다.
    // 비행 슈팅 게임에서는 0.15 ~ 0.25 사이의 값이 멀미도 안 나고 가장 쾌적합니다.
    [Range(0f, 0.5f)] 
    [SerializeField] private float mouseLeadWeight = 0.2f; 
    [SerializeField] private float maxAimDistance = 50f; // PlayerController와 동일한 기본 조준 거리
    
    [Header("Settings")]
    // 카메라가 따라가는 부드러운 시간 (낮을수록 빠름)
    [SerializeField] private float smoothTime = 0.2f;
    
    private Vector3 currentVelocity = Vector3.zero;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        // 타겟이 없으면 실행하지 않음
        if (!target)
        {
            Debug.LogWarning("[CameraController] 카메라 타겟(Player)이 지정되지 않았습니다.");
            return;
        }
        
        // 1. 현재 플레이어의 마우스 스크린 좌표 가져오기 (PlayerInput 연동)
        PlayerInput playerInput = target.GetComponent<PlayerInput>();
        Vector3 targetLookAtPoint = target.position; // 기본값은 플레이어 위치
        
        
        if (playerInput)
        {
            // 2. PlayerController에서 했던 것과 동일하게 마우스의 3D 월드 좌표 추적
            Ray ray = mainCamera.ScreenPointToRay(playerInput.MousePosition);
            Vector3 mouseWorldPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance))
            {
                mouseWorldPoint = hit.point;
            }
            else
            {
                mouseWorldPoint = ray.GetPoint(maxAimDistance);
            }

            // 3. ★ 핵심: 플레이어 위치와 마우스 3D 위치 사이의 '목표 지점' 계산
            // Vector3.Lerp를 이용해 mouseLeadWeight(예: 20%)만큼 마우스 쪽으로 치우친 중간 좌표를 구합니다.
            targetLookAtPoint = Vector3.Lerp(target.position, mouseWorldPoint, mouseLeadWeight);
        }

        // 4. 계산된 동적 목표 지점을 기준으로 카메라의 이상적인 위치 산출
        Quaternion targetRotation = Quaternion.Euler(rotationOffset);
        Vector3 rotatedOffset = targetRotation * positionOffset;
        
        // 카메라는 이제 마녀가 아니라, 마녀와 마우스 사이의 중간 지점을 추적합니다.
        Vector3 targetPosition = targetLookAtPoint + rotatedOffset;

        // 5. 부드러운 이동 및 회전 적용
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.rotation = targetRotation;
    }
}
