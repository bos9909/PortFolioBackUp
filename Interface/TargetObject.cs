using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using static MoreMountains.Feedbacks.MMF_Player;
using MoreMountains.Tools;
using Random = System.Random;

public class TargetObject : MonoBehaviour, IDamageable
{
    [Header("Start Setting")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;
    
    [Header("Visual & Sound Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("MMF Settings")] 
    [SerializeField]private MMF_Player _mmfPlayer;

    [SerializeField] private AudioClip[] myClip;
    private MMSoundManagerPlayOptions sfxOptions;
    
    private void OnEnable()
    {
        currentHealth = maxHealth;
        sfxOptions = MMSoundManagerPlayOptions.Default;
        sfxOptions.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
        sfxOptions.Loop = false; 
        sfxOptions.DoNotAutoRecycleIfNotDonePlaying = true;
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        
        // 1. 피격 이펙트 생성 (우리가 만든 만능 오브젝트 풀 사용!)
        if (hitEffectPrefab != null && ObjectPoolManager.Instance != null)
        {
            // 부딪힌 지점(hitPoint)에서 부딪힌 표면의 방향(hitNormal)을 바라보게 이펙트를 생성합니다.
            GameObject effect = ObjectPoolManager.Instance.Pop(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
        
            if (effect == null)
            {
                Debug.LogError("[디버그] 풀에서 이펙트를 꺼내오지 못했습니다! (Pop 실패)");
            }
            else
            {
                Debug.Log($"[디버그] 이펙트 풀업 성공! 이름: {effect.name}, 위치: {effect.transform.position}");
            }
        }
        
        // 2. 피격 사운드 재생 & 피격 효과 재생
        // 피격 피드백 되는데 지속시간이 너무 길다
        _mmfPlayer.PlayFeedbacks();
        MMSoundManagerSoundPlayEvent.Trigger(myClip.MMRandom(), sfxOptions);
        
        
        // 3. 체력 소진 시 파괴 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    //사망시 실행되는 함수
    private void Die()
    {
        Debug.Log($"[{gameObject.name}] 사망/파괴됨!");

        // 사망 사운드 재생  mmf로 컨트롤 하자
       // if (SoundManager.Instance != null && !string.IsNullOrEmpty(destroySoundName))
       // {
            // SoundManager.Instance.PlaySFX(destroySoundName);
       // }

        // 오브젝트 풀에서 꺼낸 적이라면 다시 풀로 반납하고, 일반 배치용 장애물이라면 그냥 파괴합니다.
        PooledObjectDespawn despawnComp = GetComponent<PooledObjectDespawn>();
        if (despawnComp != null)
        {
            despawnComp.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
