/// <summary>
/// 완제품 버퍼에서 플레이어/SalesWorker 쟁반으로 완제품을 적재하는 Zone. 버퍼 메시 감소 연출 포함.
/// 소유: 씬 내 GoodsPickupZone GameObject
/// 의존: InteractionZone, ProductionManager, InventoryComponent, MaxIndicatorUI, ObjectPool<StackMeshItem>, ObjectPool<ResourceFlyObject>
/// </summary>
/// 수정 로그:
/// 2026-05-17 OnPlayerEnter override 추가 — TutorialSystem.NotifyGoodsPickupZoneEntered() 연결
using System;
using System.Collections.Generic;
using UnityEngine;

public class GoodsPickupZone : InteractionZone
{
    // DD2: 최초 1개 이상 적재 완료 시 1회만 발행
    public static event Action OnGoodsPickupCompleted;

    [Header("버퍼 메시 적층")]
    [SerializeField] private StackMeshItem _bufferMeshPrefab;
    [SerializeField] private Transform     _bufferSocket;
    [SerializeField] private float         _stackHeight = 0.2f;
    [SerializeField] private int           _meshUnit    = 5;
    [SerializeField] private int           _poolSize    = 8;

    [Header("연출")]
    [SerializeField] private ResourceFlyObject _flyPrefab;
    [SerializeField] private int               _flyPoolSize = 10;
    [SerializeField] private float             _flyDuration = 0.4f;

    [Header("UI")]
    [SerializeField] private GameObject _maxIndicator;

    private ObjectPool<StackMeshItem>     _bufferPool;
    private ObjectPool<ResourceFlyObject> _flyPool;
    private readonly List<StackMeshItem>  _activeMeshes = new List<StackMeshItem>();
    private bool                          _hasCompletedOnce;

    protected override void Awake()
    {
        base.Awake();
        _bufferPool = new ObjectPool<StackMeshItem>(_bufferMeshPrefab, _poolSize, transform);
        _flyPool    = new ObjectPool<ResourceFlyObject>(_flyPrefab, _flyPoolSize, transform);
    }

    public override void OnPlayerEnter(PlayerCharacter player)
    {
        base.OnPlayerEnter(player);
        TutorialSystem.Instance?.NotifyEntered(TutorialSystem.ETutorialID.GoodsPickup);
    }

    public override void OnPlayerExit(PlayerCharacter player)
    {
        base.OnPlayerExit(player);
        // MAX 상태면 이탈 후에도 UI 유지 — 플레이어가 가지러 올 수 있도록
        if (ProductionManager.Instance.GoodsBuffer < ProductionManager.Instance.GoodsBufferMax)
        {
            _maxIndicator?.SetActive(false);
        }
    }

    private void Start()
    {
        if (ProductionManager.Instance != null)
        {
            ProductionManager.Instance.OnGoodsBufferChanged += HandleGoodsBufferChanged;
            HandleGoodsBufferChanged(ProductionManager.Instance.GoodsBuffer);
        }
    }

    private void OnDisable()
    {
        if (ProductionManager.Instance != null)
        {
            ProductionManager.Instance.OnGoodsBufferChanged -= HandleGoodsBufferChanged;
        }
    }

    private void HandleGoodsBufferChanged(int count)
    {
        RefreshBufferMeshes(count);

        int max = ProductionManager.Instance.GoodsBufferMax;

        if (count >= max)
        {
            _maxIndicator?.SetActive(true);
        }
        else
        {
            _maxIndicator?.SetActive(false);
        }
    }

    protected override void OnTick(PlayerCharacter player)
    {
        // MAX UI는 HandleGoodsBufferChanged에서 관리 — OnTick은 픽업 로직만 담당

        // 쟁반 MAX → 픽업 스킵
        if (player.Inventory.IsGoodsFull())
        {
            return;
        }

        // 완제품 버퍼 없음 → 픽업 스킵
        if (ProductionManager.Instance.GoodsBuffer <= 0)
        {
            return;
        }

        // 소비 전 top 위치 기록 → FlyObject 출발지
        Vector3 flyFrom = GetTopMeshPosition();

        if (!ProductionManager.Instance.ConsumeGoods(1))
        {
            return;
        }

        player.Inventory.AddGoods(1);
        SFXManager.Instance?.PlayGoodsPickup();

        // FlyObject: 꺼진 메시 위치 → 플레이어 위치 (메시 갱신은 이벤트로 처리)
        PlayFlyEffect(flyFrom, player.FlySocket.position);

        if (!_hasCompletedOnce)
        {
            _hasCompletedOnce = true;
            OnGoodsPickupCompleted?.Invoke();
        }
    }

    private void RefreshBufferMeshes(int count)
    {
        if (_bufferSocket == null)
        {
            return;
        }

        int targetCount = count / _meshUnit;

        while (_activeMeshes.Count < targetCount)
        {
            StackMeshItem item = _bufferPool.Get();
            item.transform.SetParent(_bufferSocket, false);
            _activeMeshes.Add(item);
        }

        while (_activeMeshes.Count > targetCount)
        {
            int last = _activeMeshes.Count - 1;
            _bufferPool.Return(_activeMeshes[last]);
            _activeMeshes.RemoveAt(last);
        }

        for (int i = 0; i < _activeMeshes.Count; i++)
        {
            _activeMeshes[i].transform.localPosition = new Vector3(0f, i * _stackHeight, 0f);
        }
    }

    // 현재 활성 메시 중 가장 위 위치 반환 (FlyObject 출발지)
    private Vector3 GetTopMeshPosition()
    {
        if (_bufferSocket == null)
        {
            return transform.position;
        }

        int topIndex = Mathf.Max(0, _activeMeshes.Count - 1);
        return _bufferSocket.position + new Vector3(0f, topIndex * _stackHeight, 0f);
    }

    private void PlayFlyEffect(Vector3 from, Vector3 to)
    {
        ResourceFlyObject fly = _flyPool.Get();
        fly.Fly(from, to, _flyDuration, () => _flyPool.Return(fly));
    }
}
