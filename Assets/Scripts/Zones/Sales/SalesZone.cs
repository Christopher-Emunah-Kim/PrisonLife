/// <summary>
/// 3조건 판매 틱 Zone. 조건: 트리거 안에 플레이어(or SalesWorker) + 책상 재고 ≥1 + 죄수 큐 맨 앞 대기.
/// 소유: 씬 내 SalesZone GameObject
/// 의존: BaseZone, SalesDeskZone, SalesManager, MoneyZone, PrisonerSpawner, GameBalanceData
/// </summary>
/// 수정 로그:
/// 2026-05-15 죄수 큐 조건은 MODULE-9 구현 후 연결 예정 (현재는 책상 재고 조건만 체크)
/// 2026-05-16 MODULE-9 완료 — 죄수 큐 조건 연결, 감옥 이동 방향 SerializeField 추가
using System.Collections;
using UnityEngine;

public class SalesZone : BaseZone
{
    [Header("데이터")]
    [SerializeField] private GameBalanceData _balanceData;

    [Header("참조")]
    [SerializeField] private SalesDeskZone   _deskZone;
    [SerializeField] private MoneyZone       _moneyZone;
    [SerializeField] private PrisonerSpawner _spawner;

    [Header("감옥 방향")]
    [SerializeField] private Transform _prisonTarget;  // 감옥 방향 기준 오브젝트 (씬 배치)

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
        // 조건 1: 책상 재고 ≥ 1
        if (_deskZone == null || _deskZone.DeskGoods <= 0)
        {
            return;
        }

        // 조건 2: 죄수 큐 맨 앞 대기 중
        if (_spawner == null || _spawner.FrontPrisoner == null || !_spawner.FrontPrisoner.IsWaitingInQueue)
        {
            return;
        }

        // 판매 1회 처리: 책상 -1, 죄수 말풍선 -1
        if (!_deskZone.ConsumeFromDesk(1))
        {
            return;
        }

        int goodsPrice = _balanceData != null ? _balanceData.goodsPrice : 0;
        _moneyZone?.Accumulate(goodsPrice);
        SFXManager.Instance?.PlaySales();

        Prisoner front = _spawner.FrontPrisoner;
        bool purchaseComplete = front.DecreasePurchaseCount(goodsPrice);

        if (purchaseComplete)
        {
            // 구매 완료 → 감옥 방향으로 이동
            Vector3 prisonPos = _prisonTarget != null ? _prisonTarget.position : transform.position + Vector3.forward * 10f;
        _spawner.DequeueFront(prisonPos);
            SalesManager.Instance?.NotifySaleCompleted();
        }
    }
}
