/// <summary>
/// 감옥 확장 업그레이드 Zone. 완료 시 수용 한도 40명으로 갱신 + 펜스 교체 + 게임 종료 트리거.
/// 소유: 씬 PrisonExpandZone 오브젝트 (OnPrisonFull 후 UpgradeManager가 SetActive(true))
/// 의존: UpgradeZone, PrisonManager, GameBalanceData
/// </summary>
using UnityEngine;

public class PrisonExpandZone : UpgradeZone
{
    [SerializeField] private GameBalanceData _balanceData;

    [Header("감옥 펜스 가시성")]
    [SerializeField] private GameObject _fenceOriginal;   // PrisonFenceOriginal — 초기 표시
    [SerializeField] private GameObject _fenceExpanded;   // PrisonFenceExpanded — 확장 후 표시

    protected override void InitCost()
    {
        _totalCost = _balanceData.prisonExpandCost;
    }

    protected override void OnUpgradeCompleted()
    {
        // 감옥 수용 한도 확장 → PrisonManager.OnPrisonExpanded 발행 → GameManager.TriggerGameEnd()
        if (PrisonManager.Instance != null)
        {
            PrisonManager.Instance.ExpandCapacity();
        }
        else
        {
            Logger.Error("PrisonExpandZone", "PrisonManager.Instance가 null");
        }

        // 펜스 교체
        SwapFence();

        gameObject.SetActive(false);
    }

    private void SwapFence()
    {
        if (_fenceOriginal != null)
        {
            _fenceOriginal.SetActive(false);
        }
        else
        {
            Logger.Warn("PrisonExpandZone", "PrisonFenceOriginal 참조 없음");
        }

        if (_fenceExpanded != null)
        {
            _fenceExpanded.SetActive(true);
        }
        else
        {
            Logger.Warn("PrisonExpandZone", "PrisonFenceExpanded 참조 없음");
        }
    }
}
