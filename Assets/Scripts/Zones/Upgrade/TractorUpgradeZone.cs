/// <summary>
/// 드릴 → 트랙터 업그레이드 Zone. 완료 시 채굴 레벨 2로 상향.
/// 소유: 씬 TractorUpgradeZone 오브젝트 (DrillUpgradeZone 완료 시 SetActive(true))
/// 의존: UpgradeZone, MiningZone, GameBalanceData
/// </summary>
using UnityEngine;

public class TractorUpgradeZone : UpgradeZone
{
    [SerializeField] private GameBalanceData _balanceData;
    [SerializeField] private MiningZone      _miningZone;

    protected override void InitCost()
    {
        _totalCost = _balanceData.tractorUpgradeCost;
    }

    protected override void OnUpgradeCompleted()
    {
        // 채굴 레벨 2(트랙터) 전환
        if (_miningZone != null)
        {
            _miningZone.UpgradeMiningLevel(2);
        }
        else
        {
            Logger.Warn("TractorUpgradeZone", "MiningZone 참조 없음");
        }

        // 이 Zone 비활성화
        gameObject.SetActive(false);
    }
}
