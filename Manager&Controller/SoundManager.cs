using System;
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
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
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

        bgmDictionary.Clear();
        foreach (var data in soundDataContainer.bgmList)
        {
            if (data.clip == null)
            {
                Debug.LogWarning($"[SoundManager] '{data.soundName}' 항목의 오디오 클립 파일이 비어있습니다!");
                continue;
            }
            if (!bgmDictionary.ContainsKey(data.soundName))
            {
                bgmDictionary.Add(data.soundName, data.clip);
                Debug.Log($"[SoundManager] BGM 등록 성공: {data.soundName}");
            }
        }
    }

    private void InitializeSoundManager()
    {
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
    }

    public void PlayBGM(string soundName)
    {
        Debug.Log($"[SoundManager] PlayBGM 요청 들어옴: {soundName}");

        if (bgmDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            // 최신 버전 유니티 안전장치: 클립을 새로 갈아끼우고 무조건 Play
            bgmSource.clip = clip;
            bgmSource.Play();
            
            Debug.Log($"[SoundManager] 🔊 {soundName} 실제 재생 시작됨! (오디오 소스 상태: {bgmSource.isPlaying})");
        }
        else
        {
            Debug.LogError($"[SoundManager] ❌ 딕셔너리에서 '{soundName}'을(를) 찾을 수 없습니다. 대소문자나 컨테이너 등록을 확인하세요.");
        }
    }
}