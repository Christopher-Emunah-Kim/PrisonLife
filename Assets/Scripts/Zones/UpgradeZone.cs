/// <summary>
/// 부분 지불 업그레이드 Zone 공통 베이스. 잔액은 이탈 후에도 유지.
/// 소유: DrillUpgradeZone, TractorUpgradeZone, MiningWorkerHireZone 등 (상속)
/// 의존: BaseZone, PlayerCharacter, GameBalanceData
/// </summary>
using System.Collections;
using UnityEngine;

public abstract class UpgradeZone : BaseZone
{
    [SerializeField] protected float _tickInterval = 0.05f;

    protected int  _totalCost;
    protected int  _remainingCost;
    protected bool _isCompleted;

    protected abstract void InitCost();
    protected abstract void OnUpgradeCompleted();

    protected virtual void Start()
    {
        InitCost(); //GameBalanceData참조 타이밍
        _remainingCost = _totalCost;
    }

    public override void OnPlayerEnter(PlayerCharacter player)
    {
        base.OnPlayerEnter(player);

        if (_isCompleted)
        {
            return;
        }

        StopTick();
        _tickCoroutine = StartCoroutine(UpgradeTick(player));
    }

    public override void OnPlayerExit(PlayerCharacter player)
    {
        // 잔액 유지 — zone-code.md 규칙: _remainingCost 리셋 금지
        base.OnPlayerExit(player);
    }

    private IEnumerator UpgradeTick(PlayerCharacter player)
    {
        while (_remainingCost > 0)
        {
            // 완료 플래그 재진입 방어
            if (_isCompleted)
            {
                _tickCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(_tickInterval);

            if (player == null)
            {
                Logger.Warn("UpgradeZone", "Tick 중단: player가 null");
                _tickCoroutine = null;
                yield break;
            }

            // 돈 없으면 무시, 있으면 1씩 차감
            if (!player.Inventory.ConsumeMoney(1))
            {
                continue;
            }

            _remainingCost--;

            OnCostChanged(_remainingCost);

            if (_remainingCost <= 0)
            {
                _isCompleted = true;
                StopTick();
                OnUpgradeCompleted();
                yield break;
            }
        }
    }

    // 잔여 비용 변경 시 UI 갱신 — 하위 클래스에서 선택적 오버라이드
    protected virtual void OnCostChanged(int remaining) { }
}
