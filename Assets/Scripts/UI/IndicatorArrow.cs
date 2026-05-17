/// <summary>
/// Zone 고정(Type1) 또는 플레이어 주변 Radial 방향(Type2) 인디케이터 화살표.
/// TutorialSystem이 활성/비활성 및 타입 설정을 제어한다.
/// 소유: TutorialSystem
/// 의존: 없음
/// </summary>
/// 수정 로그:
/// 2026-05-17 Type2 회전 로직 LookRotation → Z축 오일러 회전으로 변경 (World Space Canvas Image 대응)
/// 2026-05-17 Type2 Radial 배치 — 플레이어 위치 + 방향*반경으로 위치 계산, 시야 판별을 TutorialSystem으로 위임
using System.Collections;
using UnityEngine;

public class IndicatorArrow : MonoBehaviour
{
    public enum ArrowType { Zone, Player }

    [SerializeField] private ArrowType _arrowType    = ArrowType.Zone;
    [SerializeField] private float     _bobAmplitude = 0.3f;
    [SerializeField] private float     _bobSpeed     = 2f;

    [Header("Type2 전용")]
    [SerializeField] private float _radialDistance = 1.5f;  // 플레이어 중심 반경
    [SerializeField] private float _heightOffset   = 1.0f;  // 플레이어 머리 위 높이

    // Type2 전용 — TutorialSystem이 주입
    private Transform _targetZone;
    private Transform _playerTransform;

    private Coroutine _bobCoroutine;
    private bool      _isTriggered;

    private void OnEnable()
    {
        _bobCoroutine = StartCoroutine(BobRoutine());
    }

    private void OnDisable()
    {
        StopBob();
    }

    // TutorialSystem → Zone 최초 진입 완료 시 호출 (영구 제거)
    public void TriggerRemove()
    {
        if (_isTriggered)
        {
            return;
        }
        _isTriggered = true;
        gameObject.SetActive(false);
    }

    // Type2 초기화 — TutorialSystem이 활성화 전에 호출
    public void SetupPlayerArrow(Transform player, Transform target)
    {
        _playerTransform = player;
        _targetZone      = target;
    }

    private void LateUpdate()
    {
        if (_arrowType != ArrowType.Player || _targetZone == null || _playerTransform == null)
        {
            return;
        }

        UpdateRadialPosition();
        UpdateRotation();
    }

    // 플레이어 주변 반경 위에 방향별 위치 설정
    private void UpdateRadialPosition()
    {
        Vector3 toTarget = _targetZone.position - _playerTransform.position;
        toTarget.y = 0f;

        Vector3 dir = toTarget == Vector3.zero ? Vector3.forward : toTarget.normalized;
        Vector3 worldPos = _playerTransform.position
                         + dir * _radialDistance
                         + Vector3.up * _heightOffset;

        transform.position = worldPos;
    }

    // World Space Canvas Image 기준 Z축 회전 — 스프라이트 ↑가 전방
    private void UpdateRotation()
    {
        Vector3 toTarget = _targetZone.position - _playerTransform.position;
        toTarget.y = 0f;

        if (toTarget == Vector3.zero)
        {
            return;
        }

        float angle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, -angle);
    }

    // Type2는 보빙 없이 위치만 이동 — Type1만 보빙
    private IEnumerator BobRoutine()
    {
        if (_arrowType == ArrowType.Player)
        {
            yield break;
        }

        Vector3 originLocalPos = transform.localPosition;
        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.deltaTime;
            float offsetY = Mathf.Sin(elapsed * _bobSpeed) * _bobAmplitude;
            transform.localPosition = originLocalPos + new Vector3(0f, offsetY, 0f);
            yield return null;
        }
    }

    private void StopBob()
    {
        if (_bobCoroutine != null)
        {
            StopCoroutine(_bobCoroutine);
            _bobCoroutine = null;
        }
    }
}
