using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    // 외부 Controller가 읽어갈 프로퍼티
    public float MoveX { get; private set; }
    public float MoveZ { get; private set; }
    public float MoveY { get; private set; } // 상하 (Space, Ctrl 등)
    
    public Vector2 MousePosition { get; set; }
    public bool IsFiring { get; set; }
    
    //패드 조준용 벡터
    public Vector2 PadLookVector { get; private set; }
    
    //패드인지 키보드&마우스인지 판별할 플래그
    public bool IsUsingGamepad { get; private set; }
    
    private void Update()
    {
        // 매 프레임 마우스의 현재 화면 좌표를 업데이트합니다.
        if (Mouse.current != null)
        {
            MousePosition = Mouse.current.position.ReadValue();
        }
        
        //마우스가 움직이면 패드 모드는 해제
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
        {
            IsUsingGamepad = false;
        }
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        // 1. 게임 매니저 상태 체크 로그
        if (GameManager.Instance && GameManager.Instance.CurrentState != GameState.Playing)
        {
            Debug.LogWarning($"[PlayerInput] 입력이 들어왔으나, 현재 게임 상태가 Playing이 아닙니다. (현재 상태: {GameManager.Instance.CurrentState})");
            MoveX = 0f;
            MoveZ = 0f;
            return;
        }
        
        // 2. 입력 값 입력받기 (Vector2 타입으로 들어옴)
        Vector2 inputVector = context.ReadValue<Vector2>();
        MoveX = inputVector.x;
        MoveZ = inputVector.y;
        
        // 3. 입력 값 디버그 로그 (키를 누르거나 뗄 때 항상 출력됨)
        Debug.Log($"[PlayerInput] 입력 감지 -> MoveX: {MoveX:F2}, MoveZ: {MoveZ:F2}");
        
    }
    
    //패드 오른쪽 스틱 대응
    public void OnLook(InputAction.CallbackContext context)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        // 현재 입력이 패드(Gamepad)나 조이스틱 등에서 왔다면
        if (context.control.device is Gamepad)
        {
            PadLookVector = context.ReadValue<Vector2>();
            
            // 데드존(미세하게 스틱이 쏠리는 현상) 방지 
            if (PadLookVector.sqrMagnitude > 0.05f)
            {
                IsUsingGamepad = true;
            }
        }
    }
    

    public void OnUpDown(InputAction.CallbackContext context)
    {
        if (GameManager.Instance && GameManager.Instance.CurrentState != GameState.Playing)
        {
            MoveY = 0f;
            return;
        }
        
        MoveY = context.ReadValue<float>();
        Debug.Log($"[PlayerInput] 고도 제어 입력 감지 -> MoveY: {MoveY:F1}");
    }
    
    //사격 버튼(마우스 좌클릭) 입력 이벤트 구문
    public void OnFire(InputAction.CallbackContext context)
    {
        if (GameManager.Instance && GameManager.Instance.CurrentState != GameState.Playing)
        {
            IsFiring = false;
            return;
        }

        // 버튼을 누르고 있는 동안 true, 떼면 false
        if (context.started) IsFiring = true;
        else if (context.canceled) IsFiring = false;
    }

}
