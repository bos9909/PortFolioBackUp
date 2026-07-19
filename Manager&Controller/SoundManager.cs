using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sound Data")]
    [SerializeField] private SoundDataContainer soundDataContainer;

    private AudioSource bgmSource;
    private AudioSource sfxSource; // ★ 여러 개가 겹쳐서 나도록 PlayOneShot을 사용할 단일 소스
    
    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    // 사용자가 최종적으로 설정한 BGM의 실제 목표 볼륨 (기본값 1.0)
    private float maxBGMVolume = 1f;
    
    // 페이드 처리를 관리하기 위한 코루틴 변수
    private Coroutine bgmFadeCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // ★ 무조건 데이터 로드를 최우선으로 실행합니다.
            LoadSoundData(); 
            InitializeSoundManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSoundData()
    {
        if (soundDataContainer == null)
        {
            Debug.LogError("[SoundManager] SoundDataContainer 에셋이 인스펙터에 연결되지 않았습니다!");
            return;
        }

        // 1. BGM 데이터 로드
        bgmDictionary.Clear();
        foreach (var data in soundDataContainer.bgmList)
        {
            if (data.clip == null)
            {
                Debug.LogWarning($"[SoundManager] BGM '{data.soundName}' 항목의 오디오 클립 파일이 비어있습니다!");
                continue;
            }
            if (!bgmDictionary.ContainsKey(data.soundName))
            {
                bgmDictionary.Add(data.soundName, data.clip);
                Debug.Log($"[SoundManager] BGM 등록 성공: {data.soundName}");
            }
        }

        // 2. ★ SFX 데이터 로드 (누락되었던 부분 추가)
        sfxDictionary.Clear();
        foreach (var data in soundDataContainer.sfxList) // 컨테이너 내부의 sfxList 이름을 확인해 보세요!
        {
            if (data.clip == null)
            {
                Debug.LogWarning($"[SoundManager] SFX '{data.soundName}' 항목의 오디오 클립 파일이 비어있습니다!");
                continue;
            }
            if (!sfxDictionary.ContainsKey(data.soundName))
            {
                sfxDictionary.Add(data.soundName, data.clip);
                Debug.Log($"[SoundManager] SFX 등록 성공: {data.soundName}");
            }
        }
    }

    private void InitializeSoundManager()
    {
        // 1. BGM 소스 생성 및 초기화
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;

            if (audioMixer != null)
            {
                AudioMixerGroup[] bgmGroups = audioMixer.FindMatchingGroups("BGM");
                if (bgmGroups.Length > 0) bgmSource.outputAudioMixerGroup = bgmGroups[0];
            }
        }

        // 2. ★ SFX 소스 생성 및 초기화
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false; // 효과음은 반복하지 않음
            sfxSource.playOnAwake = false;

            if (audioMixer != null)
            {
                AudioMixerGroup[] sfxGroups = audioMixer.FindMatchingGroups("SFX");
                if (sfxGroups.Length > 0) sfxSource.outputAudioMixerGroup = sfxGroups[0];
            }
        }
    }

    /// <summary>
    /// BGM 그룹의 볼륨을 조절합니다 (0 ~ 1)
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        maxBGMVolume = Mathf.Clamp01(volume);

        if (audioMixer != null)
        {
            float dB = maxBGMVolume <= 0.0001f ? -80f : Mathf.Log10(maxBGMVolume) * 20f;
            audioMixer.SetFloat("BGMVolume", dB);
            bgmSource.volume = 1f; 
        }
        else
        {
            bgmSource.volume = maxBGMVolume;
        }
    }
    
    /// <summary>
    /// SFX 그룹의 볼륨을 조절합니다 (0 ~ 1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        float targetVolume = Mathf.Clamp01(volume);

        if (audioMixer != null)
        {
            float dB = targetVolume <= 0.0001f ? -80f : Mathf.Log10(targetVolume) * 20f;
            audioMixer.SetFloat("SFXVolume", dB);
            sfxSource.volume = 1f; // 오디오 믹서를 쓰므로 소스 볼륨은 기본값 유지
        }
        else
        {
            sfxSource.volume = targetVolume;
        }
    }
    
    public void PlayBGM(string soundName)
    {
        if (bgmDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            
            bgmSource.volume = maxBGMVolume;
            bgmSource.clip = clip;
            bgmSource.Play();
        }
        else
        {
            Debug.LogError($"[SoundManager] ❌ BGM '{soundName}'을(를) 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// ★ 중첩 재생이 가능한 효과음 재생 함수
    /// </summary>
    public void PlaySFX(string soundName, float localVolume = 1f)
    {
        if (sfxDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            // PlayOneShot은 동일한 오디오 소스에서 여러 클립이 서로를 간섭(끊기)하지 않고 
            // 겹쳐서 완벽하게 재생되도록 해줍니다. 연사 기능에 필수적입니다.
            sfxSource.PlayOneShot(clip, localVolume);
        }
        else
        {
            Debug.LogError($"[SoundManager] ❌ SFX '{soundName}'을(를) 찾을 수 없습니다.");
        }
    }
    
    public void PlayBGMWithFade(string soundName, float fadeDuration = 1.0f)
    {
        if (bgmDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(CoPlayBGMWithFade(clip, fadeDuration));
        }
        else
        {
            Debug.LogError($"[SoundManager] ❌ '{soundName}'을(를) 찾을 수 없습니다.");
        }
    }
    
    private IEnumerator CoPlayBGMWithFade(AudioClip newClip, float duration)
    {
        float time = 0f;

        if (bgmSource.clip && bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            while (time < duration)
            {
                time += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
                yield return null;
            }
        }

        bgmSource.volume = 0f;
        bgmSource.clip = newClip;
        bgmSource.Play();

        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, maxBGMVolume, time / duration);
            yield return null;
        }

        bgmSource.volume = maxBGMVolume;
    }
}