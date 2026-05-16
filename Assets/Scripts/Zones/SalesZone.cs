/// <summary>
/// 3조건 판매 틱 Zone. 조건: 트리거 안에 플레이어(or SalesWorker) + 책상 재고 ≥1 + 죄수 큐 맨 앞 대기.
/// 소유: 씬 내 SalesZone GameObject
/// 의존: BaseZone, SalesDeskZone, SalesManager, MoneyZone, GameBalanceData
/// </summary>
/// 수정 로그:
/// 2026-05-15 죄수 큐 조건은 MODULE-9 구현 후 연결 예정 (현재는 책상 재고 조건만 체크)
using System.Collections;
using UnityEngine;

public class SalesZone : BaseZone
{
    [Header("데이터")]
    [SerializeField] private GameBalanceData _balanceData;

    [Header("참조")]
    [SerializeField] private SalesDeskZone _deskZone;
    [SerializeField] private MoneyZone     _moneyZone;

    [Header("판매 틱")]
    [SerializeField] private float _salesTickInterval = 2f;

    // 트리거 안에 플레이어 또는 SalesWorker 존재 여부
    private bool _isOccupied;

    public override void OnPlayerEnter(PlayerCharacter player)
    {
        base.OnPlayerEnter(player);
        _isOccupied = true;
        StopTick();
        _tickCoroutine = StartCoroutine(SalesTick());
    }

    public override void OnPlayerExit(PlayerCharacter player)
    {
        base.OnPlayerExit(player);
        _isOccupied = false;
    }

    // SalesWorker용 진입/이탈 알림 (MODULE-12 구현 후 호출됨)
    public void OnWorkerEnter()
    {
        _isOccupied = true;

        if (_tickCoroutine == null)
        {
            _tickCoroutine = StartCoroutine(SalesTick());
        }
    }

    public void OnWorkerExit()
    {
        _isOccupied = false;
    }

    private IEnumerator SalesTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(_salesTickInterval);

            if (!_isOccupied)
            {
                _tickCoroutine = null;
                yield break;
            }

            TrySell();
        }
    }

    private void TrySell()
    {
        // 조건: 책상 재고 ≥ 1
        // (죄수 큐 조건은 MODULE-9 완료 후 이 메서드에 추가 예정)
        if (_deskZone == null || _deskZone.DeskGoods <= 0)
        {
            return;
        }

        if (!_deskZone.ConsumeFromDesk(1))
        {
            return;
        }

        int earnings = _balanceData != null ? _balanceData.goodsPrice : 0;
        _moneyZone?.Accumulate(earnings);
        SalesManager.Instance?.NotifySaleCompleted();
    }
}
