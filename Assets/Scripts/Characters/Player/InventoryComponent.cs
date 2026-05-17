/// <summary>
/// 플레이어 백팩(자원/돈)·쟁반(완제품) 슬롯 관리 + 더미 메시 적층 연출 컴포넌트.
/// 소유: PlayerCharacter
/// 의존: GameBalanceData, ObjectPool<StackMeshItem>
/// </summary>
/// 수정 로그:
/// 2026-05-14 백팩 자원/돈 적층 메시 + 쟁반 메시 ON/OFF 로직 추가
/// 2026-05-15 소켓 3개 + 단일 프리팹 방식으로 교체, ObjectPool 적용
/// 2026-05-17 ConsumeMoney에서 MoneyManager.Spend 연동 — HUD 갱신 누락 수정
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    // DD1: slot[0] >= MAX 50% 시 발행 (최초 1회, 리셋 없음)
    public static event Action OnResourceHalfFull;

    [SerializeField] private GameBalanceData _balanceData;

    [Header("더미 메시 프리팹 (StackMeshItem 컴포넌트 필수)")]
    [SerializeField] private StackMeshItem _resourceMeshPrefab;
    [SerializeField] private StackMeshItem _moneyMeshPrefab;
    [SerializeField] private StackMeshItem _goodsMeshPrefab;

    [Header("적층 소켓")]
    // [0]=쟁반, [1]=백팩 1번(자원 or 돈만), [2]=백팩 2번(자원+돈 동시 시 돈 시작점)
    [SerializeField] private Transform[] _stackSockets;
    [SerializeField] private float       _stackHeight = 0.2f;
    [SerializeField] private int         _meshUnit    = 5;
    [SerializeField] private int         _poolSize    = 16;

    // 백팩: slot[0]=자원, slot[1]=돈
    private int[] _backpackSlots = new int[2];
    private int[] _backpackMax   = new int[2];
    private bool  _halfFullFired;

    // 쟁반: slot[0]=완제품
    private int[] _traySlots = new int[1];
    private int   _trayMax;

    private ObjectPool<StackMeshItem> _resourcePool;
    private ObjectPool<StackMeshItem> _moneyPool;
    private ObjectPool<StackMeshItem> _goodsPool;

    // 현재 활성 메시 인스턴스
    private readonly List<StackMeshItem> _activeResourceMeshes = new List<StackMeshItem>();
    private readonly List<StackMeshItem> _activeMoneyMeshes    = new List<StackMeshItem>();
    private readonly List<StackMeshItem> _activeGoodsMeshes    = new List<StackMeshItem>();

    // 돈 메시가 현재 어느 소켓에 있는지 추적
    private int _currentMoneySocket = 1;

    public int BackpackResource    => _backpackSlots[0];
    public int BackpackMoney       => _backpackSlots[1];
    public int TrayGoods           => _traySlots[0];
    public int BackpackResourceMax => _backpackMax[0];
    public int BackpackMoneyMax    => _backpackMax[1];
    public int TrayMax             => _trayMax;
    public int MeshUnit            => _meshUnit;

    private void Awake()
    {
        _resourcePool = new ObjectPool<StackMeshItem>(_resourceMeshPrefab, _poolSize, transform);
        _moneyPool    = new ObjectPool<StackMeshItem>(_moneyMeshPrefab,    _poolSize, transform);
        _goodsPool    = new ObjectPool<StackMeshItem>(_goodsMeshPrefab,    _poolSize, transform);

        InitMaxValues(0);
    }

    public void InitMaxValues(int miningLevelIndex)
    {
        if (_balanceData == null)
        {
            Logger.Error("InventoryComponent", "GameBalanceData is null");
            return;
        }

        _backpackMax[0] = _balanceData.GetMiningLevel(miningLevelIndex).backpackMax;
        _backpackMax[1] = _balanceData.backpackMoneyMax;
        _trayMax        = _backpackMax[0];
    }

    // ── 자원 ──────────────────────────────────────────────────

    public bool AddResource(int amount)
    {
        if (_backpackSlots[0] >= _backpackMax[0])
        {
            Logger.Warn("InventoryComponent", "AddResource 무시: 자원 백팩 MAX");
            return false;
        }

        _backpackSlots[0] = Mathf.Min(_backpackSlots[0] + amount, _backpackMax[0]);
        RefreshBackpackMeshes();

        // DD1: 최초 1회만 발행, 리셋 없음
        if (!_halfFullFired && _backpackSlots[0] >= _backpackMax[0] / 2)
        {
            _halfFullFired = true;
            OnResourceHalfFull?.Invoke();
        }

        return true;
    }

    public bool ConsumeResource(int amount)
    {
        if (_backpackSlots[0] < amount)
        {
            Logger.Warn("InventoryComponent", $"ConsumeResource 실패: 보유={_backpackSlots[0]}, 요청={amount}");
            return false;
        }

        _backpackSlots[0] -= amount;
        RefreshBackpackMeshes();
        return true;
    }

    public bool IsResourceFull() => _backpackSlots[0] >= _backpackMax[0];

    // ── 돈 ───────────────────────────────────────────────────

    public bool AddMoney(int amount)
    {
        if (_backpackSlots[1] >= _backpackMax[1])
        {
            Logger.Warn("InventoryComponent", "AddMoney 무시: 돈 백팩 MAX");
            return false;
        }

        _backpackSlots[1] = Mathf.Min(_backpackSlots[1] + amount, _backpackMax[1]);
        RefreshBackpackMeshes();
        return true;
    }

    public bool ConsumeMoney(int amount)
    {
        if (_backpackSlots[1] < amount)
        {
            Logger.Warn("InventoryComponent", $"ConsumeMoney 실패: 보유={_backpackSlots[1]}, 요청={amount}");
            return false;
        }

        _backpackSlots[1] -= amount;
        RefreshBackpackMeshes();
        MoneyManager.Instance?.Subtract(amount);
        return true;
    }

    public bool IsMoneyFull() => _backpackSlots[1] >= _backpackMax[1];

    // ── 완제품(쟁반) ─────────────────────────────────────────

    public bool AddGoods(int amount)
    {
        if (_traySlots[0] >= _trayMax)
        {
            Logger.Warn("InventoryComponent", "AddGoods 무시: 쟁반 MAX");
            return false;
        }

        _traySlots[0] = Mathf.Min(_traySlots[0] + amount, _trayMax);
        RefreshTrayMeshes();
        return true;
    }

    public bool ConsumeGoods(int amount)
    {
        if (_traySlots[0] < amount)
        {
            Logger.Warn("InventoryComponent", $"ConsumeGoods 실패: 보유={_traySlots[0]}, 요청={amount}");
            return false;
        }

        _traySlots[0] -= amount;
        RefreshTrayMeshes();
        return true;
    }

    public bool IsGoodsFull() => _traySlots[0] >= _trayMax;

    // ── 메시 갱신 ────────────────────────────────────────────

    private void RefreshBackpackMeshes()
    {
        int resourceCount = _backpackSlots[0] / _meshUnit;
        int moneyCount    = _backpackSlots[1] / _meshUnit;

        // 자원 없이 돈만 있으면 소켓[1], 자원+돈 동시면 돈은 소켓[2]
        int targetMoneySocket = resourceCount > 0 ? 2 : 1;

        // 돈 소켓이 바뀌었으면 활성 돈 메시 부모 이전
        if (targetMoneySocket != _currentMoneySocket)
        {
            ReparentStack(_activeMoneyMeshes, targetMoneySocket);
            _currentMoneySocket = targetMoneySocket;
        }

        SetMeshStack(_activeResourceMeshes, _resourcePool, resourceCount, 1);
        SetMeshStack(_activeMoneyMeshes,    _moneyPool,    moneyCount,    _currentMoneySocket);
    }

    private void RefreshTrayMeshes()
    {
        int goodsCount = _traySlots[0] / _meshUnit;
        SetMeshStack(_activeGoodsMeshes, _goodsPool, goodsCount, 0);
    }

    /// <summary>
    /// activeList를 targetCount에 맞게 풀에서 Get/Return하고
    /// socketIndex 소켓 기준 localPosition으로 위치 설정.
    /// </summary>
    private void SetMeshStack(List<StackMeshItem> activeList, ObjectPool<StackMeshItem> pool, int targetCount, int socketIndex)
    {
        if (_stackSockets == null || socketIndex >= _stackSockets.Length)
        {
            return;
        }

        Transform socket = _stackSockets[socketIndex];

        if (socket == null)
        {
            return;
        }

        // 부족하면 풀에서 꺼내 소켓 자식으로 추가
        while (activeList.Count < targetCount)
        {
            StackMeshItem item = pool.Get();
            item.transform.SetParent(socket, false);
            activeList.Add(item);
        }

        // 넘치면 풀로 반환
        while (activeList.Count > targetCount)
        {
            int last = activeList.Count - 1;
            pool.Return(activeList[last]);
            activeList.RemoveAt(last);
        }

        // 위치 갱신
        for (int i = 0; i < activeList.Count; i++)
        {
            activeList[i].transform.localPosition = new Vector3(0f, i * _stackHeight, 0f);
        }
    }

    // 소켓 전환 시 기존 인스턴스 부모를 새 소켓으로 이전
    private void ReparentStack(List<StackMeshItem> activeList, int socketIndex)
    {
        if (_stackSockets == null || socketIndex >= _stackSockets.Length)
        {
            return;
        }

        Transform socket = _stackSockets[socketIndex];

        if (socket == null)
        {
            return;
        }

        for (int i = 0; i < activeList.Count; i++)
        {
            if (activeList[i] == null)
            {
                continue;
            }

            activeList[i].transform.SetParent(socket, false);
        }
    }
}
