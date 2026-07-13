using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("Target Tracking")]
    [SerializeField] private PlayerController playerController; // 마녀 오브젝트

    [Header("Settings")]
    [SerializeField] private float smoothTime = 0.05f; // 조준점 이동의 부드러운 정도 (낮을수록 칼조준)
    
    private Vector3 currentVelocity = Vector3.zero;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        
        if (playerController == null)
        {
            // 씬에 있는 PlayerController를 자동으로 탐색해 캐싱
            playerController = FindFirstObjectByType<PlayerController>();
        }
    }

    private void LateUpdate()
    {
        if (!playerController || !mainCamera) return;

        // 1. 마녀 스크립트가 실시간으로 계산하고 있는 조준점 가져오기
        Vector3 targetAimPoint = playerController.CurrentAimPoint;

        // 2. 조준점 위치로 UI(Canvas)를 부드럽게 이동
        // SmoothDamp를 사용하여 부드럽게 이동
        transform.position = Vector3.SmoothDamp(transform.position, targetAimPoint, ref currentVelocity, smoothTime);

        // 항상 카메라를 바라보게 만듦
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}