using System;
using System.Collections;
using UnityEngine;


public enum GameState
{
    None,
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    // 현재 게임 상태 (외부에서는 읽기만 가능하게 제한)
    public GameState CurrentState { get; private set; }

    //음악 재생을 위한 사운드 매니저
    SoundManager SoundManager;

    private void Awake()
    {
        // 싱글톤 세팅
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        // SoundManager를 포함한 모든 매니저가 Awake를 마치고 완벽히 준비될 때까지 1프레임 대기
        yield return null; 

        Debug.Log("[GameManager] 메인 메뉴 상태로 변경을 시도합니다.");
        ChangeState(GameState.Playing);
    }

    
    // 게임 상태를 안전하게 변경하는 함수 (핵심 메서드)
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return; // 같은 상태로의 중복 변경 방지

        // 1. 이전 상태를 빠져나갈 때 처리할 로직 (Exit Logic)
        //OnStateExit(CurrentState);

        // 상태 변경
        CurrentState = newState;

        // 2. 새로운 상태로 들어갈 때 처리할 로직 (Enter Logic)
        OnStateEnter(CurrentState);
    }
    
    private void OnStateEnter(GameState state)
    {
        Debug.Log($"[GameManager] OnStateEnter 진입 완료. 현재 내부 State: {state}");
        
        switch (state)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                //시작 음악 재생
                SoundManager.Instance.PlayBGMWithFade("BGM01", 3f);
                break;

            case GameState.Playing:
                Time.timeScale = 1f; // 게임 정상 속도
                SoundManager.Instance.PlayBGMWithFade("BGM02", 3f);
                SoundManager.Instance.SetBGMVolume(0.2f);
                break;

            case GameState.Paused:
                Time.timeScale = 0f; // 게임 일시정지 (물리, 시간 정지)
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                // 예: SoundManager.Instance.PlaySFX("GameOverSound");
                break;

            case GameState.Victory:
                Time.timeScale = 0f;
                break;
        }
    }
    
    private void OnStateExit(GameState state)
    {
        // 상태가 바뀔 때 청소하거나 초기화해야 할 청소 로직이 있다면 여기서 처리
        switch (state)
        {
            case GameState.Paused:
                // 일시정지를 풀고 나갈 때 다시 게임 속도를 정상으로 돌리는 등의 처리 가능
                break;
        }
    }
}
