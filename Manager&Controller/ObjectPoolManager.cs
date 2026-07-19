using System.Collections.Generic; 
using UnityEngine;                
using UnityEngine.Pool;           

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Header("Pool Capacity Settings")]
    [SerializeField] private int defaultCapacity = 20; // 풀 개설 시 기본 생성 용량
    [SerializeField] private int maxPoolSize = 100;    // 풀이 가질 수 있는 기본 최대 오브젝트 개수

    // 프리팹별로 각각의 유니티 내장 풀(창고)을 관리하는 딕셔너리
    private Dictionary<GameObject, IObjectPool<GameObject>> poolDictionary = new Dictionary<GameObject, IObjectPool<GameObject>>();

    // 꺼내진 자식 오브젝트가 어떤 원본 프리팹 출신인지 기록하는 반납용 장부
    private Dictionary<GameObject, GameObject> instanceToPrefabMap = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        // 싱글톤 초기화 로직
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 오브젝트 풀에서 원하는 프리팹을 하나 꺼내 활성화하는 함수 (Pop)
    /// </summary>
    public GameObject Pop(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        // 1. 해당 프리팹 전용 창고(풀)가 없다면 즉석에서 새로 개설
        if (!poolDictionary.ContainsKey(prefab))
        {
            CreateNewPool(prefab);
        }

        // 2. 해당 프리팹 전용 풀에서 인스턴스를 하나 꺼내옴
        GameObject instance = poolDictionary[prefab].Get();
        
        // 3. 위치 및 회전값 세팅 후 반환
        instance.transform.position = position;
        instance.transform.rotation = rotation;

        instance.SetActive(true);
        
        return instance;
    }

    /// <summary>
    /// 다 쓴 오브젝트를 풀(창고)에 안전하게 비활성화하여 반납하는 함수 (Push)
    /// </summary>
    public void Push(GameObject instance)
    {
        if (instance == null) return;

        // 1. 반납 장부를 확인해서 이 녀석이 어떤 프리팹 출신인지 원본을 탐색
        if (instanceToPrefabMap.TryGetValue(instance, out GameObject originalPrefab))
        {
            // 2. 원본 프리팹 전용 창고에 안전하게 반납 처리
            if (poolDictionary.ContainsKey(originalPrefab))
            {
                poolDictionary[originalPrefab].Release(instance);
            }
        }
        else
        {
            // 만약 장부에 없는 오브젝트라면 풀링 대상이 아니므로 일반 파괴 처리
            Destroy(instance);
        }
    }

    /// <summary>
    /// 특정 프리팹 전용 유니티 내장 풀을 동적으로 개설하는 내부 함수
    /// </summary>
    private void CreateNewPool(GameObject prefab)
    {
        IObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
            // 1. 풀에 오브젝트가 모자랄 때 실행할 생성 규칙
            () => {
                GameObject instance = Instantiate(prefab);
                instance.transform.SetParent(this.transform); // 매니저 자식으로 정렬
                instanceToPrefabMap[instance] = prefab;      // 장부에 출신 성분 기록
                return instance;
            },
            // 2. 풀에서 꺼낼 때 실행할 규칙 총알이 휘는 문제 때문에 활성화는 따로 처리
            (instance) => {},
            // 3. 풀에 반납받을 때 실행할 규칙
            (instance) => instance.SetActive(false),
            // 4. maxPoolSize 제한을 넘어가서 넘쳐나는 오브젝트를 버릴 때 실행할 규칙
            (instance) => {
                instanceToPrefabMap.Remove(instance);
                Destroy(instance);
            },
            true,            // collectionCheck: 중복 반납 방지 예외 처리 활성화
            defaultCapacity, // 최초 할당 용량
            maxPoolSize      // 최대 허용 용량
        );

        // 완성된 전용 풀을 딕셔너리에 최종 등록
        poolDictionary.Add(prefab, newPool);
    }
}