/// <summary>
/// 판매 틱마다 판매금을 누적하고, 플레이어 진입 시 전액을 백팩으로 이전하는 Zone.
/// 소유: 씬 내 MoneyZone GameObject
/// 의존: InteractionZone, InventoryComponent, MoneyManager, ResourceFlyObject, DOTween
/// </summary>
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoneyZone : InteractionZone
{
    // 판매금 Zone 누적 변경 이벤트 (기획서 05_Zone_Sales.md)
    public static event Action<int> OnMoneyZoneAccumulated;

    [Header("연출")]
    [SerializeField] private ResourceFlyObject _flyPrefab;
    [SerializeField] private int               _flyPoolSize = 10;
    [SerializeField] private float             _flyDuration = 0.4f;

    [Header("버퍼 메시 적층")]
    [SerializeField] private StackMeshItem _moneyMeshPrefab;
    [SerializeField] private Transform     _moneySocket;
    [SerializeField] private float         _stackHeight = 0.2f;
    [SerializeField] private int           _meshUnit    = 5;
    [SerializeField] private int           _poolSize    = 8;

    private ObjectPool<ResourceFlyObject> _flyPool;
    private ObjectPool<StackMeshItem>     _moneyPool;
    private readonly List<StackMeshItem>  _activeMeshes = new List<StackMeshItem>();

    private int _accumulatedMoney;

    private void Awake()
    {
        _flyPool   = new ObjectPool<ResourceFlyObject>(_flyPrefab, _flyPoolSize, transform);
        _moneyPool = new ObjectPool<StackMeshItem>(_moneyMeshPrefab, _poolSize, transform);
    }


    // SalesZone이 판매 틱마다 호출
    public void Accumulate(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _accumulatedMoney += amount;
        RefreshMoneyMeshes();
        OnMoneyZoneAccumulated?.Invoke(_accumulatedMoney);
    }

    protected override void OnTick(PlayerCharacter player)
    {
        // 누적 금액 없음 → 스킵
        if (_accumulatedMoney <= 0)
        {
            return;
        }

        // 백팩 돈 MAX → 스킵
        if (player.Inventory.IsMoneyFull())
        {
            return;
        }

        // 이전 가능한 금액 계산 (백팩 남은 용량 이내)
        int capacity  = player.Inventory.BackpackMoneyMax - player.Inventory.BackpackMoney;
        int transfer  = Mathf.Min(_accumulatedMoney, capacity);

        if (transfer <= 0)
        {
            return;
        }

        Vector3 flyFrom = _moneySocket != null ? _moneySocket.position : transform.position;

        _accumulatedMoney -= transfer;
        RefreshMoneyMeshes();
        OnMoneyZoneAccumulated?.Invoke(_accumulatedMoney);

        // DOTween 흡수 연출: Zone → 플레이어
        PlayFlyEffect(flyFrom, player.FlySocket.position);

        player.Inventory.AddMoney(transfer);
        MoneyManager.Instance?.Add(transfer);
    }

    private void RefreshMoneyMeshes()
    {
        if (_moneySocket == null)
        {
            return;
        }

        int targetCount = _accumulatedMoney / _meshUnit;

        while (_activeMeshes.Count < targetCount)
        {
            StackMeshItem item = _moneyPool.Get();
            item.transform.SetParent(_moneySocket, false);
            _activeMeshes.Add(item);
        }

        while (_activeMeshes.Count > targetCount)
        {
            int last = _activeMeshes.Count - 1;
            _moneyPool.Return(_activeMeshes[last]);
            _activeMeshes.RemoveAt(last);
        }

        for (int i = 0; i < _activeMeshes.Count; i++)
        {
            _activeMeshes[i].transform.localPosition = new Vector3(0f, i * _stackHeight, 0f);
        }
    }

    private void PlayFlyEffect(Vector3 from, Vector3 to)
    {
        ResourceFlyObject fly = _flyPool.Get();
        fly.Fly(from, to, _flyDuration, () => _flyPool.Return(fly));
    }
}
