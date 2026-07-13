using UnityEngine;

public class PooledObjectDespawn : MonoBehaviour
{
    [Header("Despawn Settings")]
    [SerializeField] private float autoDespawnTime = 2f; // 자동으로 반납될 시간 (초)
    [SerializeField] private bool useParticleDuration = false; // 파티클 시스템 길이에 맞출지 여부

    private float deactivateTime;
    private ParticleSystem cachedParticleSystem;

    private void Awake()
    {
        // 만약 파티클 이펙트라면 파티클 컴포넌트를 미리 캐싱
        if (useParticleDuration)
        {
            cachedParticleSystem = GetComponent<ParticleSystem>();
            if (cachedParticleSystem == null)
            {
                cachedParticleSystem = GetComponentInChildren<ParticleSystem>();
            }
        }
    }

    private void OnEnable()
    {
        // 1. 파티클 시스템 길이를 기반으로 반납 시간을 정하는 모드라면
        if (useParticleDuration && cachedParticleSystem != null)
        {
            // 파티클의 메인 모듈에서 설정된 지속 시간(Duration)을 자동으로 가져옵니다.
            deactivateTime = Time.time + cachedParticleSystem.main.duration;
        }
        else
        {
            // 2. 일반적인 총알이나 지정된 초 단위로 반납하는 모드
            deactivateTime = Time.time + autoDespawnTime;
        }
    }

    private void Update()
    {
        // 매 프레임 체크하여 시간이 다 되면 풀로 자진 반납
        if (Time.time >= deactivateTime)
        {
            Despawn();
        }
    }

    /// <summary>
    /// 오브젝트를 파괴하지 않고 만능 풀 매니저에게 되돌려주는 함수
    /// </summary>
    public void Despawn()
    {
        if (ObjectPoolManager.Instance)
        {
            // 이 스크립트가 붙은 오브젝트 자신(gameObject)을 통째로 풀에 반납합니다.
            ObjectPoolManager.Instance.Push(this.gameObject);
        }
        else
        {
            // 안전장치
            Destroy(gameObject);
        }
    }
}