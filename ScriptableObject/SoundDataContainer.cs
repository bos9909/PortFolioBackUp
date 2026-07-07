using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundData
{
    public string soundName; // 코드로 호출할 사운드 이름
    public AudioClip clip;   // 실제 사운드 파일
}

[CreateAssetMenu(fileName = "SoundDataContainer", menuName = "Audio/SoundDataContainer")]
public class SoundDataContainer : ScriptableObject
{
    public List<SoundData> bgmList = new List<SoundData>();
    public List<SoundData> sfxList = new List<SoundData>();
}
