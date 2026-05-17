/// <summary>
/// 플레이어 즉시 추적 + 컷씬 Coroutine (목표 위치 이동 → 대기 → 복귀 → onComplete 콜백).
/// 컷씬 타이밍/Zone 활성화 책임은 UpgradeManager / PrisonExpandZone에 있음.
/// 소유: 씬 Main Camera 오브젝트
/// 의존: PlayerCharacter
/// </summary>
/// 수정 로그:
/// 2026-05-17 OnFirstSaleCompleted / OnPrisonFull 직접 구독 제거 → PlayCutscene(pos, Action) 공개 API로 교체
using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private Vector3         _offset            = new Vector3(0f, 10f, -8f);
    [SerializeField] private float           _cutsceneLerpSpeed = 5f;
    [SerializeField] private float           _holdDuration      = 1.5f;
    [SerializeField] private float           _returnDuration    = 0.8f;

    private bool      _isCutscenePlaying;
    private Coroutine _cutsceneCoroutine;

    private void Awake()
    {
        if (_player == null)
        {
            Logger.Error("CameraController", "Player 참조 없음");
        }
    }

    private void LateUpdate()
    {
        if (_isCutscenePlaying || _player == null)
        {
            return;
        }

        transform.position = _player.transform.position + _offset;
    }

    // ── 공개 API ─────────────────────────────────────────

    /// <summary>
    /// 컷씬 재생. 목표 위치로 lerp 이동 → holdDuration 대기 → 플레이어 복귀 → onComplete 호출.
    /// 이미 컷씬 재생 중이면 무시 (01_Overview: 두 컷씬은 동시 발생 불가).
    /// </summary>
    public void PlayCutscene(Vector3 targetWorldPos, Action onComplete = null)
    {
        if (_isCutscenePlaying)
        {
            Logger.Warn("CameraController", "컷씬 재생 중 — 중복 요청 무시");
            return;
        }

        _cutsceneCoroutine = StartCoroutine(CutsceneRoutine(targetWorldPos, onComplete));
    }

    // ── 컷씬 Coroutine ───────────────────────────────────

    private IEnumerator CutsceneRoutine(Vector3 targetWorldPos, Action onComplete)
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
            float   elapsed  = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < _returnDuration)
            {
                elapsed += Time.deltaTime;
                float   t            = Mathf.Clamp01(elapsed / _returnDuration);
                Vector3 returnTarget = _player.transform.position + _offset;
                transform.position   = Vector3.Lerp(startPos, returnTarget, t);
                yield return null;
            }
        }

        // 4. 콜백 호출 → Zone 활성화 등 후속 처리
        onComplete?.Invoke();

        _isCutscenePlaying = false;
        _cutsceneCoroutine = null;

        if (_player != null)
        {
            _player.SetInputBlocked(false);
        }
    }
}
