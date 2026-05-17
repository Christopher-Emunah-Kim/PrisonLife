/// <summary>
/// 판매 완료 이벤트 및 첫 판매 트리거를 발행하는 판매 관리자.
/// 소유: SalesZone (판매 틱 완료 시 호출)
/// 의존: Singleton<T>
/// </summary>
/// 수정 로그:
/// 2026-05-17 Singleton<T> 베이스 클래스 적용
using System;
using UnityEngine;

public class SalesManager : Singleton<SalesManager>
{
    public event Action OnSalesCompleted;
    public event Action OnFirstSaleCompleted;

    private bool _firstSaleDone;

    // SalesZone이 판매 1회 완료 시 호출
    public void NotifySaleCompleted()
    {
        OnSalesCompleted?.Invoke();

        if (!_firstSaleDone)
        {
            _firstSaleDone = true;
            OnFirstSaleCompleted?.Invoke();
        }
    }
}
