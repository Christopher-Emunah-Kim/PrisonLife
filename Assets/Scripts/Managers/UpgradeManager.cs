/// <summary>
/// 업그레이드 Zone의 활성화 순서와 단계 전환을 관리.
/// - 첫 판매 완료 → DrillUpgradeZone / SalesWorkerHireZone 활성화
/// - DrillUpgradeZone 완료 → TractorUpgradeZone / MiningWorkerHireZone 동시 활성화
/// - 감옥 만원 → PrisonExpandZone 활성화
/// 소유: 씬 루트 (단일 인스턴스)
/// 의존: SalesManager, PrisonManager, Singleton<T>
/// </summary>
/// 수정 로그:
/// 2026-05-17 MiningWorkerHireZone 활성화 타이밍을 FirstSale → DrillCompleted로 변경
/// 2026-05-17 Singleton<T> 베이스 클래스 적용
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
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

    private void HandleFirstSaleCompleted()
    {
        SetActiveIfNotNull(_drillUpgradeZone,    true);
        SetActiveIfNotNull(_salesWorkerHireZone, true);
    }

    private void HandlePrisonFull()
    {
        SetActiveIfNotNull(_prisonExpandZone, true);
    }

    /// <summary>DrillUpgradeZone 완료 시 호출 — Drill 비활성화, Tractor + MiningWorkerHireZone 활성화.</summary>
    public void NotifyDrillCompleted()
    {
        SetActiveIfNotNull(_drillUpgradeZone,     false);
        SetActiveIfNotNull(_tractorUpgradeZone,   true);
        SetActiveIfNotNull(_miningWorkerHireZone, true);
    }

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
