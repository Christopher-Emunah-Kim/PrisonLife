/// <summary>
/// 판매 완료 이벤트 및 첫 판매 트리거를 발행하는 판매 관리자.
/// 소유: SalesZone (판매 틱 완료 시 호출)
/// 의존: 없음 (이벤트 발행 전용)
/// </summary>
using System;
using UnityEngine;

public class SalesManager : MonoBehaviour
{
    public static event Action OnSalesCompleted;
    public static event Action OnFirstSaleCompleted;

    public static SalesManager Instance { get; private set; }

    private bool _firstSaleDone;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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
