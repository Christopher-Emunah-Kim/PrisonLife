/// <summary>
/// 플레이어 즉시 추적 + 컷씬 Coroutine (목표 Zone 이동 → 대기 → 복귀).
/// 소유: 씬 Main Camera 오브젝트
/// 의존: PlayerCharacter, SalesManager, PrisonManager
/// </summary>
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private Vector3         _offset            = new Vector3(0f, 10f, -8f);
    [SerializeField] private float           _cutsceneLerpSpeed = 5f;
    [SerializeField] private float           _holdDuration      = 1.5f;
    [SerializeField] private float           _returnDuration    = 0.8f;

    // 컷씬 재생 중 플레이어 추적 및 입력 차단
    private bool _isCutscenePlaying;

    private void Awake()
    {
        if (_player == null)
        {
            Logger.Error("CameraController", "Player 참조 없음");
        }
    }

    private void Start()
    {
        if (SalesManager.Instance != null)
        {
            SalesManager.Instance.OnFirstSaleCompleted += HandleFirstSaleCompleted;
        }

        if (PrisonManager.Instance != null)
        {
            PrisonManager.Instance.OnPrisonFull += HandlePrisonFull;
        }
    }

    private void OnDisable()
    {
        if (SalesManager.Instance != null)
        {
            SalesManager.Instance.OnFirstSaleCompleted -= HandleFirstSaleCompleted;
        }

        if (PrisonManager.Instance != null)
        {
            PrisonManager.Instance.OnPrisonFull -= HandlePrisonFull;
        }
    }

    private void LateUpdate()
    {
        if (_isCutscenePlaying || _player == null)
        {
            return;
        }

        // 플레이어 즉시 추적 
        transform.position = _player.transform.position + _offset;
    }

    // ── 컷씬 트리거 ──────────────────────────────────────

    // 컷씬 대상 Zone Transform은 GameManager나 각 Zone에서 참조를 넘겨주는 방식 대신
    // Inspector에 미리 연결해 두는 방식 사용 (씬 고정 구조)
    [SerializeField] private Transform _drillUpgradeZoneTransform;
    [SerializeField] private Transform _prisonExpandZoneTransform;

    private void HandleFirstSaleCompleted()
    {
        if (_drillUpgradeZoneTransform == null)
        {
            return;
        }

        StartCutscene(_drillUpgradeZoneTransform.position);
    }

    private void HandlePrisonFull()
    {
        if (_prisonExpandZoneTransform == null)
        {
            return;
        }

        StartCutscene(_prisonExpandZoneTransform.position);
    }

    private void StartCutscene(Vector3 targetWorldPos)
    {
        if (_isCutscenePlaying)
        {
            return;
        }

        StartCoroutine(CutsceneRoutine(targetWorldPos));
    }

    // ── 컷씬 Coroutine ───────────────────────────────────

    private IEnumerator CutsceneRoutine(Vector3 targetWorldPos)
    {
        _isCutscenePlaying = true;

        if (_player != null)
        {
            _player.SetInputBlocked(true);
        }

        // 1. 목표 위치로 lerp 이동
        Vector3 targetCamPos = targetWorldPos + _offset;

        while (Vector3.Distance(transform.position, targetCamPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetCamPos,
                _cutsceneLerpSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetCamPos;

        // 2. 대기
        yield return new WaitForSeconds(_holdDuration);

        // 3. 플레이어 위치로 복귀
        if (_player != null)
        {
            float elapsed    = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < _returnDuration)
            {
                elapsed            += Time.deltaTime;
                float t             = Mathf.Clamp01(elapsed / _returnDuration);
                Vector3 returnTarget = _player.transform.position + _offset;
                transform.position  = Vector3.Lerp(startPos, returnTarget, t);
                yield return null;
            }
        }

        _isCutscenePlaying = false;

        if (_player != null)
        {
            _player.SetInputBlocked(false);
        }
    }
}
