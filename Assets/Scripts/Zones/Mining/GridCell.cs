/// <summary>
/// 채굴 그리드의 단일 셀. 채굴 가능 여부·예약 상태 관리 + DOTween 연출.
/// 소유: MiningGrid
/// 의존: DOTween, GameBalanceData (리젠 시간은 MiningGrid가 관리)
/// </summary>
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GridCell : MonoBehaviour
{
    [SerializeField] private float _scaleDownDuration = 0.2f;

    public bool IsMinable  => _isMinable;
    public bool IsReserved => _isReserved;

    private bool _isMinable  = true;
    private bool _isReserved = false;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    // ── 예약 ─────────────────────────────────────────────

    public void Reserve()
    {
        _isReserved = true;
    }

    public void CancelReserve()
    {
        _isReserved = false;
    }

    // ── 채굴 ─────────────────────────────────────────────

    /// <summary>셀 채굴 처리. DOTween 스케일 축소 후 비활성화.</summary>
    public void Mine()
    {
        if (!_isMinable)
        {
            return;
        }

        _isMinable  = false;
        _isReserved = false;

        transform.DOScale(Vector3.zero, _scaleDownDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false));
    }

    // ── 리젠 ─────────────────────────────────────────────

    /// <summary>MiningGrid에서 regenTime 대기 후 호출.</summary>
    public void Regenerate()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;

        transform.DOScale(_originalScale, _scaleDownDuration)
            .SetEase(Ease.OutBack);

        _isMinable  = true;
        _isReserved = false;
    }
}
