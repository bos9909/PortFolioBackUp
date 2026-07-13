using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 25f;       // 총알이 날아가는 속도
    [SerializeField] private float lifeTime = 2f;    // 몇 초 뒤에 자동으로 풀에 반납될지
    [SerializeField] private GameObject hitPrefab; // 어딘가에 부딪혔을 때 불러올 이펙트 프리팹
    
    private float deactivateTime;
    private PooledObjectDespawn despawnComponent; //반납용 스크립트

    private void Awake()
    {
        despawnComponent = GetComponent<PooledObjectDespawn>();
    }

    // 오브젝트 풀에서 꺼내져서 활성화(SetActive(true))될 때마다 Start 대신 이 함수가 매번 실행됩니다.
    private void OnEnable()
    {
        // 켜진 시간 기준으로 생존 만료 시간 계산 (현재 시간 + 유지 시간)
        deactivateTime = Time.time + lifeTime;
    }

    private void Update()
    {
        // 1. 자신의 '앞방향' 기준으로 전진
        // 마녀가 조준점을 향해 회전시켜준 빗자루 끝(FirePoint)의 방향 그대로 직진합니다.
        transform.Translate(Vector3.forward * (speed * Time.deltaTime), Space.Self);

        // 2. 설정한 생존 시간이 다 되면 풀(창고)로 반납합니다.
        if (Time.time >= deactivateTime)
        {
            despawnComponent.Despawn();
        }
    }

    // 3. 충돌 처리 (Is Trigger가 체크된 Collider가 필요합니다)
    private void OnTriggerEnter(Collider other)
    {
        // 무언가에 부딪혔다면, 내가 직접 풀을 부르는 게 아니라 
        // 내 몸에 붙어있는 만능 반납 컴포넌트에게 "지금 즉시 반납해줘!" 라고 토스합니다.
        if (despawnComponent != null)
        {
            despawnComponent.Despawn();
        }
        else
        {
            // 혹시 안 붙여두었을 때를 대비한 안전장치 일반 반납
            ObjectPoolManager.Instance?.Push(this.gameObject);
        }
    }
    
}