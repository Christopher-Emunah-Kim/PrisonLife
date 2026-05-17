/// <summary>
/// 감옥 수용 인원을 추적하고 만원/확장 이벤트를 발행하는 감옥 관리자.
/// 소유: PrisonZone (TryAdmit 호출), PrisonExpandZone (ExpandCapacity 호출)
/// 의존: GameBalanceData, Singleton<T>
/// </summary>
/// 수정 로그:
/// 2026-05-17 Singleton<T> 베이스 클래스 적용
using System;
using UnityEngine;

public class PrisonManager : Singleton<PrisonManager>
{
    public event Action      OnPrisonFull;
    public event Action      OnPrisonExpanded;
    public event Action<int> OnPrisonCountChanged;

    [SerializeField] private GameBalanceData _balanceData;

    private int _currentCount;
    private int _capacity;

    public int  CurrentCount => _currentCount;
    public int  Capacity     => _capacity;
    public bool IsFull       => _currentCount >= _capacity;

    protected override void OnAwake()
    {
        _capacity = _balanceData.prisonInitialCapacity;
    }

    // PrisonZone이 죄수 수용 시 호출
    public bool TryAdmit()
    {
        if (IsFull)
        {
            return false;
        }

        _currentCount++;
        OnPrisonCountChanged?.Invoke(_currentCount);

        if (IsFull)
        {
            OnPrisonFull?.Invoke();
        }
        return true;
    }

    // PrisonExpandZone 완료 시 호출
    public void ExpandCapacity()
    {
        _capacity = _balanceData.prisonExpandedCapacity;
        OnPrisonExpanded?.Invoke();
        OnPrisonCountChanged?.Invoke(_currentCount);
    }
}
