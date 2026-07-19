using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 25f;       // 총알이 날아가는 속도
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 2f;    // 몇 초 뒤에 자동으로 풀에 반납될지
    [SerializeField] private GameObject hitPrefab; // 어딘가에 부딪혔을 때 불러올 이펙트 프리팹
    
    private float deactivateTime;
    private PooledObjectDespawn despawnComponent; //반납용 스크립트
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        despawnComponent = GetComponent<PooledObjectDespawn>();

        if (rb)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    // 오브젝트 풀에서 꺼내져서 활성화(SetActive(true))될 때마다 Start 대신 이 함수가 매번 실행됩니다.
    private void OnEnable()
    {
        
    }

    private void Update()
    {
        // 1. 코드로 조준된 정면(Forward)을 향해 묵직하게 직진시킵니다. (휘어질 틈이 없습니다)
        transform.Translate(Vector3.forward * (speed * Time.deltaTime), Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 상대방(other)에게서 데미지 인터페이스를 찾습니다.
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // 트리거는 충돌 점(ContactPoint)을 제공하지 않으므로, 
            // 가장 가까운 가상의 충돌 위치와 노말 값을 계산해서 넘겨줍니다.
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 contactNormal = (transform.position - contactPoint).normalized;

            damageable.TakeDamage(damage, contactPoint, contactNormal);
        }

        // 총알 반납
        if (despawnComponent != null) despawnComponent.Despawn();
        else ObjectPoolManager.Instance?.Push(this.gameObject);
    }
    
    
    // private void OnCollisionEnter(Collision collision)
    // {
    //     // 1. 부딪힌 대상에게 "때릴 수 있는 기능(인터페이스)"이 있는지 확인합니다.
    //     IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
    //
    //     if (damageable != null)
    //     {
    //         // 첫 번째 충돌 지점의 상세 좌표와 충돌면의 방향(Normal)을 가져옵니다.
    //         ContactPoint contact = collision.contacts[0];
    //         
    //         // 데미지 함수 호출! (상대방이 적인지 나무상자인지 총알은 알 필요가 없습니다.)
    //         damageable.TakeDamage(damage, contact.point, contact.normal);
    //     }
    //
    //     // 2. 총알 자체는 풀에 반납합니다.
    //     if (despawnComponent != null)
    //     {
    //         despawnComponent.Despawn();
    //     }
    //     else
    //     {
    //         ObjectPoolManager.Instance?.Push(this.gameObject);
    //     }
    // }
    
}