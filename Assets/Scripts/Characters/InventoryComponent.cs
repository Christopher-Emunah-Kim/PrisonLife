/// <summary>
/// 플레이어 백팩(자원/돈)·쟁반(완제품) 슬롯 관리 컴포넌트.
/// 소유: PlayerController
/// 의존: GameBalanceData
/// </summary>
using System;
using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    // DD1: slot[0] >= MAX 50% 시 발행
    public static event Action OnResourceHalfFull;

    [SerializeField] private GameBalanceData _balanceData;
    [SerializeField] private int             _meshUnit = 5;

    // 백팩: slot[0]=자원, slot[1]=돈
    private int[] _backpackSlots    = new int[2];
    private int[] _backpackMax      = new int[2];
    private bool  _halfFullFired;   // DD1: OnResourceHalfFull 중복 발행 방지

    // 쟁반: slot[0]=완제품
    private int[] _traySlots = new int[1];
    private int   _trayMax;

    // 더미 메시 표시용 (외부 읽기 전용)
    public int BackpackResource => _backpackSlots[0];
    public int BackpackMoney    => _backpackSlots[1];
    public int TrayGoods        => _traySlots[0];

    public int BackpackResourceMax => _backpackMax[0];
    public int BackpackMoneyMax    => _backpackMax[1];
    public int TrayMax             => _trayMax;

    public int MeshUnit => _meshUnit;

    private void Awake()
    {
        InitMaxValues(0); // 기본 채굴 1단계 기준 초기화
    }

    /// <summary>
    /// 채굴 단계 업그레이드 시 MAX 재설정. miningLevelIndex: 0-based.
    /// </summary>
    public void InitMaxValues(int miningLevelIndex)
    {
        if (_balanceData == null)
        {
            Logger.Error("InventoryComponent", "GameBalanceData is null");
            return;
        }

        _backpackMax[0] = _balanceData.GetMiningLevel(miningLevelIndex).backpackMax;
        _backpackMax[1] = _balanceData.backpackMoneyMax;
        _trayMax        = _backpackMax[0]; // 쟁반 MAX = 백팩 자원 MAX와 동일
    }

    // ── 자원 ──────────────────────────────────────────────────

    /// <summary>자원 추가. MAX 도달 시 무시. DD1 임계치 체크 포함.</summary>
    public bool AddResource(int amount)
    {
        if (_backpackSlots[0] >= _backpackMax[0])
        {
            Logger.Warn("InventoryComponent", "AddResource 무시: 자원 백팩 MAX");
            return false;
        }

        _backpackSlots[0] = Mathf.Min(_backpackSlots[0] + amount, _backpackMax[0]);

        // DD1: 50% 도달 시 최초 1회만 발행, 자원 소비로 50% 미만이 되면 플래그 리셋
        if (!_halfFullFired && _backpackSlots[0] >= _backpackMax[0] / 2)
        {
            _halfFullFired = true;
            OnResourceHalfFull?.Invoke();
        }

        return true;
    }

    /// <summary>자원 소비. 보유량이 부족하면 false.</summary>
    public bool ConsumeResource(int amount)
    {
        if (_backpackSlots[0] < amount)
        {
            Logger.Warn("InventoryComponent", $"ConsumeResource 실패: 보유={_backpackSlots[0]}, 요청={amount}");
            return false;
        }

        _backpackSlots[0] -= amount;
        return true;
    }

    public bool IsResourceFull()
    {
        return _backpackSlots[0] >= _backpackMax[0];
    }

    // ── 돈 ───────────────────────────────────────────────────

    public bool AddMoney(int amount)
    {
        if (_backpackSlots[1] >= _backpackMax[1])
        {
            Logger.Warn("InventoryComponent", "AddMoney 무시: 돈 백팩 MAX");
            return false;
        }

        _backpackSlots[1] = Mathf.Min(_backpackSlots[1] + amount, _backpackMax[1]);
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
        return true;
    }

    public bool IsMoneyFull()
    {
        return _backpackSlots[1] >= _backpackMax[1];
    }

    // ── 완제품(쟁반) ─────────────────────────────────────────

    public bool AddGoods(int amount)
    {
        if (_traySlots[0] >= _trayMax)
        {
            Logger.Warn("InventoryComponent", "AddGoods 무시: 쟁반 MAX");
            return false;
        }

        _traySlots[0] = Mathf.Min(_traySlots[0] + amount, _trayMax);
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
        return true;
    }

    public bool IsGoodsFull()
    {
        return _traySlots[0] >= _trayMax;
    }
}
