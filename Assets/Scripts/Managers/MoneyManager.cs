/// <summary>
/// 플레이어 소지금을 관리하고 변경 이벤트를 발행하는 금전 관리자.
/// 소유: MoneyZone (Add 호출), UpgradeZone (Spend 호출)
/// 의존: 없음
/// </summary>
using System;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static event Action<int> OnMoneyChanged;

    public static MoneyManager Instance { get; private set; }

    private int _currentMoney;
    public int CurrentMoney => _currentMoney;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }
        
        _currentMoney += amount;
        OnMoneyChanged?.Invoke(_currentMoney);
    }

    public bool Spend(int amount)
    {
        if (amount <= 0 || _currentMoney < amount)
        {
            return false;
        }
        
        _currentMoney -= amount;
        OnMoneyChanged?.Invoke(_currentMoney);
        return true;
    }
}
