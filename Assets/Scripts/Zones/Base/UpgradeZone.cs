/// <summary>
/// 부분 지불 업그레이드 Zone 공통 베이스. 잔액은 이탈 후에도 유지.
/// 틱마다 돈 차감 성공 시 플레이어→Zone 방향 DOTween 흡수 연출 + 프로그레스바 갱신.
/// 소유: DrillUpgradeZone, TractorUpgradeZone, MiningWorkerHireZone 등 (상속)
/// 의존: BaseZone, PlayerCharacter, GameBalanceData, ResourceFlyObject, UpgradeProgressUI, DOTween
/// </summary>
/// 수정 로그:
/// 2026-05-17 DOTween 돈 흡수 연출 추가 (ResourceFlyObject 풀링)
/// 2026-05-17 UpgradeProgressUI 연동 추가
/// 2026-05-17 WaitForSeconds 캐싱 (_tickWait)
using System.Collections;
using UnityEngine;

public abstract class UpgradeZone : BaseZone
{
    [SerializeField] protected float _tickInterval = 0.05f;

    [Header("돈 흡수 연출")]
    [SerializeField] private ResourceFlyObject _flyPrefab;
    [SerializeField] private int               _flyPoolSize = 10;
    [SerializeField] private float             _flyDuration = 0.3f;

    [Header("프로그레스 UI")]
    [SerializeField] private UpgradeProgressUI _progressUI;

    protected int  _totalCost;
    protected int  _remainingCost;
    protected bool _isCompleted;

    private ObjectPool<ResourceFlyObject> _flyPool;
    private WaitForSeconds _tickWait;

    protected abstract void InitCost();
    protected abstract void OnUpgradeCompleted();

    protected virtual void Awake()
    {
        if (_flyPrefab != null)
        {
            _flyPool = new ObjectPool<ResourceFlyObject>(_flyPrefab, _flyPoolSize, transform);
        }

        _tickWait = new WaitForSeconds(_tickInterval);
    }

    protected virtual void Start()
    {
        InitCost();
        _remainingCost = _totalCost;

        if (_progressUI != null)
        {
            _progressUI.Initialize(_totalCost);
        }
    }

    public override void OnPlayerEnter(PlayerCharacter player)
    {
        base.OnPlayerEnter(player);

        if (_isCompleted)
        {
            return;
        }

        StopTick();
        _tickCoroutine = StartCoroutine(UpgradeTick(player));
    }

    public override void OnPlayerExit(PlayerCharacter player)
    {
        // 잔액 유지 — zone-code.md 규칙: _remainingCost 리셋 금지
        base.OnPlayerExit(player);
    }

    private IEnumerator UpgradeTick(PlayerCharacter player)
    {
        while (_remainingCost > 0)
        {
            if (_isCompleted)
            {
                _tickCoroutine = null;
                yield break;
            }

            yield return _tickWait;

            if (player == null)
            {
                Logger.Warn("UpgradeZone", "Tick 중단: player가 null");
                _tickCoroutine = null;
                yield break;
            }

            if (!player.Inventory.ConsumeMoney(1))
            {
                continue;
            }

            _remainingCost--;
            SFXManager.Instance?.PlayUpgradePay();

            // 돈 차감 성공 → 흡수 연출 (플레이어 FlySocket → Zone 중심)
            PlayFlyEffect(player.FlySocket.position, transform.position);

            OnCostChanged(_remainingCost);

            if (_remainingCost <= 0)
            {
                _isCompleted = true;
                StopTick();
                SFXManager.Instance?.PlayUpgradeComplete();
                OnUpgradeCompleted();
                yield break;
            }
        }
    }

    private void PlayFlyEffect(Vector3 from, Vector3 to)
    {
        if (_flyPool == null)
        {
            return;
        }

        ResourceFlyObject fly = _flyPool.Get();
        fly.Fly(from, to, _flyDuration, () => _flyPool.Return(fly));
    }

    protected virtual void OnCostChanged(int remaining)
    {
        if (_progressUI != null)
        {
            _progressUI.UpdateProgress(remaining, _totalCost);
        }
    }
}
