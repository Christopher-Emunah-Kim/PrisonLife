/// <summary>
/// 플레이어 소지금을 관리하고 변경 이벤트를 발행하는 금전 관리자.
/// 소유: MoneyZone (Add 호출), UpgradeZone (Spend 호출)
/// 의존: Singleton<T>
/// </summary>
/// 수정 로그:
/// 2026-05-17 Singleton<T> 베이스 클래스 적용
using System;
using UnityEngine;

public class MoneyManager : Singleton<MoneyManager>
{
    public event Action<int> OnMoneyChanged;

    private int _currentMoney;
    public int CurrentMoney => _currentMoney;

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
