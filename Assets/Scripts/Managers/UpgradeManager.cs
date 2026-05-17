/// <summary>
/// 업그레이드 Zone의 활성화 순서와 단계 전환을 관리.
/// - 첫 판매 완료 → DrillUpgradeZone / MiningWorkerHireZone / SalesWorkerHireZone 동시 활성화
/// - DrillUpgradeZone 완료 → TractorUpgradeZone 활성화 (같은 위치에서 교체)
/// - 감옥 만원 → PrisonExpandZone 활성화
/// 소유: 씬 루트 (단일 인스턴스)
/// 의존: SalesManager, PrisonManager
/// </summary>
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("첫 판매 완료 시 동시 활성화")]
    [SerializeField] private GameObject _drillUpgradeZone;
    [SerializeField] private GameObject _miningWorkerHireZone;
    [SerializeField] private GameObject _salesWorkerHireZone;

    [Header("드릴 완료 시 교체 (동일 위치 전환)")]
    // DrillUpgradeZone.OnUpgradeCompleted → NotifyDrillCompleted() 호출
    [SerializeField] private GameObject _tractorUpgradeZone;

    [Header("감옥 만원 시 활성화")]
    [SerializeField] private GameObject _prisonExpandZone;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        SalesManager.OnFirstSaleCompleted += HandleFirstSaleCompleted;
        PrisonManager.OnPrisonFull        += HandlePrisonFull;
    }

    private void OnDisable()
    {
        SalesManager.OnFirstSaleCompleted -= HandleFirstSaleCompleted;
        PrisonManager.OnPrisonFull        -= HandlePrisonFull;
    }

    // 첫 판매 완료 → Drill / HireZone 3종 동시 활성화
    private void HandleFirstSaleCompleted()
    {
        SetActiveIfNotNull(_drillUpgradeZone,     true);
        SetActiveIfNotNull(_miningWorkerHireZone, true);
        SetActiveIfNotNull(_salesWorkerHireZone,  true);
    }

    // 감옥 만원 → PrisonExpandZone 활성화
    private void HandlePrisonFull()
    {
        SetActiveIfNotNull(_prisonExpandZone, true);
    }

    /// <summary>DrillUpgradeZone 완료 시 호출 — Drill 비활성화, Tractor 활성화.</summary>
    public void NotifyDrillCompleted()
    {
        SetActiveIfNotNull(_drillUpgradeZone,  false);
        SetActiveIfNotNull(_tractorUpgradeZone, true);
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
