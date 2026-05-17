/// <summary>
/// ETutorialID 기반 튜토리얼 상태 머신. Zone 진입 시 현재 화살표 OFF + 다음 화살표 ON.
/// 소유: 씬 내 Managers GameObject
/// 의존: IndicatorArrow
/// </summary>
/// 수정 로그:
/// 2026-05-17 ETutorialID 기반으로 전면 재작성 — 이벤트 구독 제거, Zone 진입 기준 통일
/// 2026-05-17 Singleton<T> 베이스 클래스 적용
using UnityEngine;

public class TutorialSystem : Singleton<TutorialSystem>
{
    public enum ETutorialID
    {
        Mining, 
        Drop, 
        GoodsPickup, 
        SalesDesk, 
        Money, 
        Done
    }

    [Header("플레이어 참조")]
    [SerializeField] private Transform _playerTransform;

    [Header("Zone 고정 화살표 (Type1) — 순서: Mining/Drop/GoodsPickup/SalesDesk/Money")]
    [SerializeField] private IndicatorArrow _miningArrow;
    [SerializeField] private IndicatorArrow _dropArrow;
    [SerializeField] private IndicatorArrow _goodsPickupArrow;
    [SerializeField] private IndicatorArrow _salesDeskArrow;
    [SerializeField] private IndicatorArrow _moneyArrow;

    [Header("플레이어 주변 Radial 화살표 (Type2)")]
    [SerializeField] private IndicatorArrow _playerArrow;

    [Header("각 Zone Transform (Type2 방향 계산용)")]
    [SerializeField] private Transform _miningZoneTransform;
    [SerializeField] private Transform _dropZoneTransform;
    [SerializeField] private Transform _goodsPickupZoneTransform;
    [SerializeField] private Transform _salesDeskZoneTransform;
    [SerializeField] private Transform _moneyZoneTransform;

    private ETutorialID _currentID = ETutorialID.Mining;
    private bool        _playerArrowEnabled;

    private void Start()
    {
        // 게임 시작 — Mining 화살표 ON
        ShowArrow(ETutorialID.Mining);
    }

    private void Update()
    {
        if (!_playerArrowEnabled || _playerArrow == null)
        {
            return;
        }

        // 현재 타겟 Zone이 시야 안이면 OFF, 밖이면 ON
        Transform target = GetZoneTransform(_currentID);
        if (target == null)
        {
            return;
        }

        bool inView = IsInViewport(target);
        _playerArrow.gameObject.SetActive(!inView);
    }

    // ── Zone 진입 알림 — 각 Zone의 OnPlayerEnter에서 호출 ────────

    public void NotifyEntered(ETutorialID id)
    {
        if (_currentID != id)
        {
            return;
        }

        // 현재 화살표 OFF
        GetArrow(id)?.TriggerRemove();

        // 다음 단계로
        ETutorialID next = id + 1;
        _currentID = next;

        if (next == ETutorialID.Done)
        {
            SetPlayerArrowEnabled(false);
            return;
        }

        ShowArrow(next);
    }

    // ── 내부 헬퍼 ────────────────────────────────────────────────

    private void ShowArrow(ETutorialID id)
    {
        IndicatorArrow arrow = GetArrow(id);
        if (arrow != null)
        {
            arrow.gameObject.SetActive(true);
        }

        Transform target = GetZoneTransform(id);
        if (_playerArrow != null && _playerTransform != null && target != null)
        {
            _playerArrow.SetupPlayerArrow(_playerTransform, target);
            SetPlayerArrowEnabled(true);
        }
    }

    private void SetPlayerArrowEnabled(bool enabled)
    {
        _playerArrowEnabled = enabled;
        if (!enabled && _playerArrow != null)
        {
            _playerArrow.gameObject.SetActive(false);
        }
    }

    private IndicatorArrow GetArrow(ETutorialID id)
    {
        switch (id)
        {
            case ETutorialID.Mining:      return _miningArrow;
            case ETutorialID.Drop:        return _dropArrow;
            case ETutorialID.GoodsPickup: return _goodsPickupArrow;
            case ETutorialID.SalesDesk:   return _salesDeskArrow;
            case ETutorialID.Money:       return _moneyArrow;
            default:                      return null;
        }
    }

    private Transform GetZoneTransform(ETutorialID id)
    {
        switch (id)
        {
            case ETutorialID.Mining:      return _miningZoneTransform;
            case ETutorialID.Drop:        return _dropZoneTransform;
            case ETutorialID.GoodsPickup: return _goodsPickupZoneTransform;
            case ETutorialID.SalesDesk:   return _salesDeskZoneTransform;
            case ETutorialID.Money:       return _moneyZoneTransform;
            default:                      return null;
        }
    }

    private bool IsInViewport(Transform target)
    {
        if (Camera.main == null)
        {
            return false;
        }
        Vector3 vp = Camera.main.WorldToViewportPoint(target.position);
        return vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;
    }
}
