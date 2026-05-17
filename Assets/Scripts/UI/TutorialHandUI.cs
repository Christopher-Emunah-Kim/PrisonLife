/// <summary>
/// 게임 시작 시 화면 하단 중앙에 조작 유도 UI(인피니티 심볼+손가락) 표시.
/// 첫 터치 입력 감지 시 영구 비활성.
/// 소유: HUDCanvas 자식 오브젝트
/// 의존: JoystickController.OnJoystickInput
/// </summary>
using UnityEngine;

public class TutorialHandUI : MonoBehaviour
{
    private void OnEnable()
    {
        JoystickController.OnJoystickInput += HandleInput;
    }

    private void OnDisable()
    {
        JoystickController.OnJoystickInput -= HandleInput;
    }

    private void HandleInput(Vector2 input)
    {
        // 데드존 이상 입력이 들어오면 영구 비활성
        if (input.sqrMagnitude > 0f)
        {
            gameObject.SetActive(false);
        }
    }
}
