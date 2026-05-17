/// <summary>
/// Zone 고정(Type1) 또는 플레이어 주변 3D Radial 방향(Type2) 인디케이터 화살표.
/// TutorialSystem이 활성/비활성 및 타입 설정을 제어한다.
/// 소유: TutorialSystem
/// 의존: 없음
/// </summary>
/// 수정 로그:
/// 2026-05-17 Type2 회전 로직 LookRotation → Z축 오일러 회전으로 변경 (World Space Canvas Image 대응)
/// 2026-05-17 Type2 Radial 배치 — 플레이어 위치 + 방향*반경으로 위치 계산, 시야 판별을 TutorialSystem으로 위임
/// 2026-05-17 World Space Canvas Image → 3D 메시로 전환 — LookAt 기반 회전으로 단순화
using System.Collections;
using UnityEngine;

public class IndicatorArrow : MonoBehaviour
{
    public enum ArrowType { Zone, Player }

    [SerializeField] private ArrowType _arrowType    = ArrowType.Zone;
    [SerializeField] private float     _bobAmplitude = 0.3f;
    [SerializeField] private float     _bobSpeed     = 2f;

    [Header("Type2 전용")]
    [SerializeField] private float _radialDistance = 1.5f;   // 플레이어 중심 반경
    [SerializeField] private float _heightOffset   = 1.0f;   // 플레이어 머리 위 높이
    [SerializeField] private float _orbitSpeed     = 90f;    // 공전 속도 (도/초), 0이면 고정

    // Type2 전용 — TutorialSystem이 주입
    private Transform _targetZone;
    private Transform _playerTransform;

    // 현재 궤도 각도 (degrees, XZ 평면)
    private float _orbitAngle;

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

        // 새 타겟 설정 시 궤도 각도를 현재 타겟 방향으로 초기화
        if (player != null && target != null)
        {
            Vector3 toTarget = target.position - player.position;
            toTarget.y = 0f;
            if (toTarget != Vector3.zero)
            {
                _orbitAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
            }
        }
    }

    private void LateUpdate()
    {
        if (_arrowType != ArrowType.Player || _targetZone == null || _playerTransform == null)
        {
            return;
        }

        UpdateOrbitAngle();
        UpdateRadialPosition();
        UpdateRotation();
    }

    // 공전 각도를 타겟 방향으로 점점 당기면서 회전
    private void UpdateOrbitAngle()
    {
        // 타겟 방향 각도 계산
        Vector3 toTarget = _targetZone.position - _playerTransform.position;
        toTarget.y = 0f;
        if (toTarget == Vector3.zero)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;

        if (_orbitSpeed > 0f)
        {
            // 현재 각도 → 타겟 방향으로 orbitSpeed 속도로 이동 (항상 짧은 경로)
            _orbitAngle = Mathf.MoveTowardsAngle(_orbitAngle, targetAngle, _orbitSpeed * Time.deltaTime);
        }
        else
        {
            _orbitAngle = targetAngle;
        }
    }

    // 궤도 각도 기준으로 플레이어 주변 반경 위치에 배치
    private void UpdateRadialPosition()
    {
        float rad = _orbitAngle * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));

        transform.position = _playerTransform.position
                           + dir * _radialDistance
                           + Vector3.up * _heightOffset;
    }

    // 3D 메시 기준 — 타겟을 향해 LookAt (업 벡터: 글로벌 Up)
    private void UpdateRotation()
    {
        Vector3 toTarget = _targetZone.position - _playerTransform.position;
        toTarget.y = 0f;

        if (toTarget == Vector3.zero)
        {
            return;
        }

        // 화살표 +Z(Forward)가 타겟 방향을 가리키도록 회전
        transform.rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
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
