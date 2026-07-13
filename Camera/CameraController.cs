using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Position Offset (Distance)")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 7f, -9f);

    [Header("Rotation Settings (Angles)")]
    [SerializeField] private Vector3 rotationOffset = new Vector3(35f, 0f, 0f);

    [Header("Mouse/Pad Lead Settings")]
    [Range(0f, 0.5f)] 
    [SerializeField] private float aimLeadWeight = 0.2f; // 마우스와 패드 공용 웨이트

    [Header("Settings")]
    [SerializeField] private float smoothTime = 0.08f;
    
    private Vector3 currentVelocity = Vector3.zero;
    private PlayerController playerController;

    private void Start()
    {
        if (target != null)
        {
            playerController = target.GetComponent<PlayerController>();
        }
    }

    private void LateUpdate()
    {
        if (!target) return;
        if (!playerController) playerController = target.GetComponent<PlayerController>();

        Vector3 targetLookAtPoint = target.position;

        // ★ 마녀가 마우스든 패드든 현재 계산하고 있는 '진짜 3D 조준점'이 있다면
        if (playerController)
        {
            // 마녀 위치와 조준점 사이의 내분점을 카메라의 타겟으로 설정합니다.
            targetLookAtPoint = Vector3.Lerp(target.position, playerController.CurrentAimPoint, aimLeadWeight);
        }

        // 카메라 위치 및 회전 연산
        Quaternion targetRotation = Quaternion.Euler(rotationOffset);
        Vector3 rotatedOffset = targetRotation * positionOffset;
        Vector3 targetPosition = targetLookAtPoint + rotatedOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.rotation = targetRotation;
    }
}