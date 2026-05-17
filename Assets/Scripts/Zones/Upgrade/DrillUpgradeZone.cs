/// <summary>
/// 곡괭이 → 드릴 업그레이드 Zone. 완료 시 채굴 레벨 1로 상향.
/// Zone 전환(비활성화/Tractor 활성화)은 UpgradeManager.NotifyDrillCompleted()에 위임.
/// 소유: 씬 DrillUpgradeZone 오브젝트
/// 의존: UpgradeZone, MiningZone, GameBalanceData, UpgradeManager
/// </summary>
using UnityEngine;

public class DrillUpgradeZone : UpgradeZone
{
    [SerializeField] private GameBalanceData _balanceData;
    [SerializeField] private MiningZone      _miningZone;

    protected override void InitCost()
    {
        _totalCost = _balanceData.drillUpgradeCost;
    }

    protected override void OnUpgradeCompleted()
    {
        if (_miningZone != null)
        {
            _miningZone.UpgradeMiningLevel(1);
        }
        else
        {
            Logger.Warn("DrillUpgradeZone", "MiningZone 참조 없음");
        }

        // Drill 비활성화 + Tractor 활성화는 UpgradeManager가 처리
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.NotifyDrillCompleted();
        }
        else
        {
            Logger.Warn("DrillUpgradeZone", "UpgradeManager.Instance가 null — Zone 전환 불가");
        }
    }
}
