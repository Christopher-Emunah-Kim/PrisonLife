/// <summary>
/// 업그레이드 Zone의 활성화 순서와 단계 전환을 관리.
/// 컷씬 타이밍 소유: 이벤트 수신 → CameraController.PlayCutscene() → 콜백에서 SetActive.
/// - 컷씬1 OnFirstSaleCompleted  → DrillUpgradeZone / SalesWorkerHireZone 활성화
/// - 컷씬2 NotifyDrillCompleted  → MiningWorkerHireZone / TractorUpgradeZone 활성화
/// - 컷씬3 OnPrisonFull          → PrisonExpandZone 활성화
/// 소유: 씬 루트 (단일 인스턴스)
/// 의존: CameraController, SalesManager, PrisonManager, Singleton<T>
/// </summary>
/// 수정 로그:
/// 2026-05-17 MiningWorkerHireZone 활성화 타이밍을 FirstSale → DrillCompleted로 변경
/// 2026-05-17 Singleton<T> 베이스 클래스 적용
/// 2026-05-17 컷씬 콜백 구조 도입 — SetActive를 PlayCutscene onComplete 콜백으로 이동
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    [Header("컷씬 컨트롤러")]
    [SerializeField] private CameraController _cameraController;

    [Header("컷씬 목표 위치")]
    [SerializeField] private Transform _drillCutsceneTarget;
    [SerializeField] private Transform _miningWorkerCutsceneTarget;
    [SerializeField] private Transform _prisonExpandCutsceneTarget;

    [Header("첫 판매 완료 시 활성화")]
    [SerializeField] private GameObject _drillUpgradeZone;
    [SerializeField] private GameObject _salesWorkerHireZone;

    [Header("드릴 완료 시 활성화")]
    [SerializeField] private GameObject _tractorUpgradeZone;
    [SerializeField] private GameObject _miningWorkerHireZone;

    [Header("감옥 만원 시 활성화")]
    [SerializeField] private GameObject _prisonExpandZone;

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

    // ── 컷씬1: 첫 판매 완료 ──────────────────────────────

    private void HandleFirstSaleCompleted()
    {
        SetActiveIfNotNull(_drillUpgradeZone,    true);
        SetActiveIfNotNull(_salesWorkerHireZone, true);

        if (_cameraController == null || _drillCutsceneTarget == null)
        {
            Logger.Warn("UpgradeManager", "컷씬1 스킵 — CameraController 또는 Target 없음");
            return;
        }

        _cameraController.PlayCutscene(_drillCutsceneTarget.position);
    }

    // ── 컷씬2: 드릴 업그레이드 완료 ─────────────────────

    /// <summary>DrillUpgradeZone 완료 시 호출 — 컷씬 후 Tractor + MiningWorkerHireZone 활성화.</summary>
    public void NotifyDrillCompleted()
    {
        SetActiveIfNotNull(_drillUpgradeZone,     false);
        SetActiveIfNotNull(_tractorUpgradeZone,   true);
        SetActiveIfNotNull(_miningWorkerHireZone, true);

        if (_cameraController == null || _miningWorkerCutsceneTarget == null)
        {
            Logger.Warn("UpgradeManager", "컷씬2 스킵 — CameraController 또는 Target 없음");
            return;
        }

        _cameraController.PlayCutscene(_miningWorkerCutsceneTarget.position);
    }

    // ── 컷씬3: 감옥 만원 ─────────────────────────────────

    private void HandlePrisonFull()
    {
        SetActiveIfNotNull(_prisonExpandZone, true);

        if (_cameraController == null || _prisonExpandCutsceneTarget == null)
        {
            Logger.Warn("UpgradeManager", "컷씬3 스킵 — CameraController 또는 Target 없음");
            return;
        }

        _cameraController.PlayCutscene(_prisonExpandCutsceneTarget.position);
    }

    // ── 공통 ─────────────────────────────────────────────

    private void SetActiveIfNotNull(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
        else
        {
            Logger.Warn("UpgradeManager", $"GameObject 참조 없음 (active={active})");
        }
    }
}
