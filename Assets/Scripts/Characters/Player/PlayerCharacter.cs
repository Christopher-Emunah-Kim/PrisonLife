/// <summary>
/// BaseCharacter 상속 플레이어. 조이스틱 입력 수신 + Inventory 보유.
/// Zone 진입/이탈은 BaseZone 측 OnTriggerEnter/Exit에서 처리.
/// 소유: 씬 Player 오브젝트
/// 의존: BaseCharacter, InventoryComponent, JoystickController
/// </summary>
/// 수정 로그:
/// 2026-05-15 FlySocket 추가 (ResourceFlyObject 출발/도착 위치)
using UnityEngine;

[RequireComponent(typeof(InventoryComponent))]
public class PlayerCharacter : BaseCharacter
{
    public InventoryComponent Inventory { get; private set; }

    [SerializeField] private Animator   _animator;
    [SerializeField] private Transform  _flySocket;  // ResourceFlyObject 출발/도착 위치 (백팩 높이)

    public Transform FlySocket => _flySocket != null ? _flySocket : transform;
    
    // 조이스틱 델타 (XY 스크린 → XZ 월드 변환)
    private Vector2 _inputDelta;

    private void Awake()
    {
        Inventory = GetComponent<InventoryComponent>();
    }

    private void OnEnable()
    {
        JoystickController.OnJoystickInput += HandleJoystickInput;
    }

    private void OnDisable()
    {
        JoystickController.OnJoystickInput -= HandleJoystickInput;
    }

    private void Update()
    {
        // 스크린 XY → 월드 XZ 변환 후 이동
        Vector3 worldDir = new Vector3(_inputDelta.x, 0f, _inputDelta.y);
        Move(worldDir);
        
        // 이동 속도에 따라 애니메이션 전환
        _animator.SetFloat("Speed", worldDir.magnitude);
    }

    private void HandleJoystickInput(Vector2 delta)
    {
        _inputDelta = delta;
    }
    
    // 채굴 시
    public void PlayMineAnimation()
    {
        _animator.SetTrigger("Mine");
    }
}
