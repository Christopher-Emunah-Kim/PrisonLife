/// <summary>
/// 화면 하단 50% 영역 동적 조이스틱. New Input System 기반.
/// 소유: PlayerController
/// 의존: UnityEngine.InputSystem (EnhancedTouch)
/// </summary>
using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class JoystickController : MonoBehaviour
{
    public static event Action<Vector2> OnJoystickInput;

    [SerializeField] private float _deadzone = 0.1f;

    // 조이스틱 UI 루트 (Inspector 연결 — 없으면 비주얼 없이 입력만 동작)
    [SerializeField] private RectTransform _joystickRoot;
    [SerializeField] private RectTransform _joystickKnob;

    private int   _activeFingerId = -1;
    private Vector2 _startPos;

    // 조이스틱 물리 반경 (픽셀)
    [SerializeField] private float _joystickRadius = 80f;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += HandleFingerDown;
        Touch.onFingerMove += HandleFingerMove;
        Touch.onFingerUp   += HandleFingerUp;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= HandleFingerDown;
        Touch.onFingerMove -= HandleFingerMove;
        Touch.onFingerUp   -= HandleFingerUp;
        EnhancedTouchSupport.Disable();
    }

    private void HandleFingerDown(Finger finger)
    {
        // 이미 추적 중인 터치가 있으면 무시
        if (_activeFingerId != -1)
        {
            return;
        }

        // 화면 하단 50%만 조이스틱 영역으로 허용
        if (!IsInJoystickArea(finger.screenPosition))
        {
            return;
        }

        _activeFingerId = finger.index;
        _startPos       = finger.screenPosition;
        SetJoystickVisible(true, _startPos);
    }

    private void HandleFingerMove(Finger finger)
    {
        if (finger.index != _activeFingerId)
        {
            return;
        }

        Vector2 delta = finger.screenPosition - _startPos;
        float   radius = _joystickRadius;

        // 데드존 이내 → 입력 없음
        if (delta.magnitude < _deadzone * radius)
        {
            OnJoystickInput?.Invoke(Vector2.zero);
            UpdateKnobPosition(Vector2.zero);
            return;
        }

        // 반경 클램프 후 정규화
        Vector2 clamped = Vector2.ClampMagnitude(delta, radius);
        Vector2 normalized = clamped / radius;

        OnJoystickInput?.Invoke(normalized);
        UpdateKnobPosition(clamped);
    }

    private void HandleFingerUp(Finger finger)
    {
        if (finger.index != _activeFingerId)
        {
            return;
        }

        _activeFingerId = -1;
        OnJoystickInput?.Invoke(Vector2.zero);
        SetJoystickVisible(false, Vector2.zero);
    }

    private bool IsInJoystickArea(Vector2 screenPos)
    {
        return screenPos.y < Screen.height * 0.5f;
    }

    private void SetJoystickVisible(bool visible, Vector2 screenPos)
    {
        if (_joystickRoot == null)
        {
            return;
        }

        _joystickRoot.gameObject.SetActive(visible);

        if (visible)
        {
            _joystickRoot.position = screenPos;
        }

        UpdateKnobPosition(Vector2.zero);
    }

    private void UpdateKnobPosition(Vector2 offset)
    {
        if (_joystickKnob == null)
        {
            return;
        }

        _joystickKnob.anchoredPosition = offset;
    }
}
