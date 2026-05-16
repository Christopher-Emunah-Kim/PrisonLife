/// <summary>
/// 판매 대기 죄수. 직선 이동 + 말풍선 UI 참조. 풀링 대상.
/// 소유: PrisonerSpawner (ObjectPool 관리)
/// 의존: BaseCharacter, PrisonerBubbleUI, PrisonZone
/// </summary>
using UnityEngine;

public class Prisoner : BaseCharacter, IPoolable
{
    [Header("참조")]
    [SerializeField] private GameObject _bubbleRoot;    // 말풍선 UI 루트
    [SerializeField] private PrisonerBubbleUI _bubbleUI;

    // 이동 목적지 (SalesZone 앞 대기 위치 또는 PrisonZone 방향)
    private Vector3 _moveTarget;
    private bool    _isMoving;
    private bool    _hasReachedTarget;

    // 구매 수량 (SalesZone이 판매 시 참조)
    public int PurchaseCount { get; private set; }
    // 총 지불 금액
    public int TotalPayment  { get; private set; }

    // 현재 상태
    public bool IsWaitingInQueue { get; private set; }

    public void Initialize()
    {
        _inputBlocked     = false;
        _isMoving         = false;
        _hasReachedTarget = false;
        IsWaitingInQueue  = false;
        PurchaseCount     = 0;
        TotalPayment      = 0;
        SetBubbleVisible(false);
    }

    /// <summary>판매 틱마다 SalesZone이 호출. 수량 0 되면 true 반환 → dequeue 신호.</summary>
    public bool DecreasePurchaseCount(int goodsPrice)
    {
        if (PurchaseCount <= 0)
        {
            return true;
        }

        PurchaseCount--;
        TotalPayment = PurchaseCount * goodsPrice;
        if (_bubbleUI != null)
        {
            _bubbleUI.SetProgressCount(PurchaseCount);
        }
        return PurchaseCount <= 0;
    }

    /// <summary>판매 대기 죄수 초기화. 스폰 시 PrisonerSpawner가 호출.</summary>
    public void Setup(int purchaseCount, int goodsPrice, Vector3 queuePosition)
    {
        PurchaseCount = purchaseCount;
        TotalPayment  = purchaseCount * goodsPrice;
        SetMoveTarget(queuePosition);
        IsWaitingInQueue = false;
        SetBubbleCount(purchaseCount);
    }

    public void SetMoveTarget(Vector3 target)
    {
        _moveTarget       = target;
        _moveTarget.y     = transform.position.y; // Y축 고정
        _isMoving         = true;
        _hasReachedTarget = false;
    }

    /// <summary>큐 도착 → 대기 상태로 전환. PrisonerSpawner가 호출.</summary>
    public void OnReachedQueue()
    {
        _isMoving        = false;
        IsWaitingInQueue = true;
    }

    /// <summary>판매 완료 후 감옥 방향으로 이동 시작. 목적지 월드 위치를 직접 전달.</summary>
    public void MoveTowardPrison(Vector3 targetPosition)
    {
        IsWaitingInQueue  = false;
        _isMoving         = true;
        _hasReachedTarget = false;
        _moveTarget       = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        SetBubbleVisible(false);
    }

    public void HideBubble()
    {
        SetBubbleVisible(false);
    }

    public void ShowNoCellBubble(bool show)
    {
        // No Cell 표시 시 BubbleRoot가 꺼져 있어도 강제 활성화
        if (show)
        {
            SetBubbleVisible(true);
        }

        if (_bubbleUI != null)
        {
            _bubbleUI.ShowNoCell(show);
        }
    }

    private void Update()
    {
        if (!_isMoving || _hasReachedTarget)
        {
            return;
        }

        Vector3 dir = (_moveTarget - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            _hasReachedTarget = true;
            _isMoving         = false;
            return;
        }

        Move(dir.normalized);
    }

    private void SetBubbleCount(int count)
    {
        SetBubbleVisible(true);
        if (_bubbleUI != null)
        {
            _bubbleUI.SetCount(count);
        }
    }

    private void SetBubbleVisible(bool visible)
    {
        if (_bubbleRoot != null)
        {
            _bubbleRoot.SetActive(visible);
        }
    }
}
