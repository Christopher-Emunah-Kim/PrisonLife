/// <summary>
/// 플레이어 백팩 자원을 자원 버퍼로 투입하는 Drop Zone. 버퍼 메시 적층 연출 포함.
/// 소유: 씬 내 ResourceDropZone GameObject
/// 의존: InteractionZone, ProductionManager, InventoryComponent, MaxIndicatorUI, ObjectPool<StackMeshItem>, ObjectPool<ResourceFlyObject>
/// </summary>
/// 수정 로그:
/// 2026-05-17 OnPlayerEnter override 추가 — TutorialSystem.NotifyDropZoneEntered() 연결
using System.Collections.Generic;
using UnityEngine;

public class ResourceDropZone : InteractionZone
{
    [Header("버퍼 메시 적층")]
    [SerializeField] private StackMeshItem _bufferMeshPrefab;
    [SerializeField] private Transform     _bufferSocket;     // 메시 쌓일 기준점
    [SerializeField] private float         _stackHeight = 0.2f;
    [SerializeField] private int           _meshUnit    = 5;
    [SerializeField] private int           _poolSize    = 8;

    [Header("연출")]
    [SerializeField] private ResourceFlyObject _flyPrefab;
    [SerializeField] private int               _flyPoolSize = 10;
    [SerializeField] private float             _flyDuration = 0.4f;

    [Header("UI")]
    [SerializeField] private GameObject _maxIndicator;

    private ObjectPool<StackMeshItem>    _bufferPool;
    private ObjectPool<ResourceFlyObject> _flyPool;
    private readonly List<StackMeshItem> _activeMeshes = new List<StackMeshItem>();

    protected override void Awake()
    {
        base.Awake();
        _bufferPool = new ObjectPool<StackMeshItem>(_bufferMeshPrefab, _poolSize, transform);
        _flyPool    = new ObjectPool<ResourceFlyObject>(_flyPrefab, _flyPoolSize, transform);
    }

    public override void OnPlayerEnter(PlayerCharacter player)
    {
        base.OnPlayerEnter(player);
        TutorialSystem.Instance?.NotifyEntered(TutorialSystem.ETutorialID.Drop);
    }

    public override void OnPlayerExit(PlayerCharacter player)
    {
        base.OnPlayerExit(player);
        // MAX 상태면 이탈 후에도 UI 유지
        if (ProductionManager.Instance.ResourceBuffer < ProductionManager.Instance.ResourceBufferMax)
        {
            _maxIndicator?.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ProductionManager.Instance.OnResourceBufferChanged += HandleResourceBufferChanged;
    }

    private void OnDisable()
    {
        ProductionManager.Instance.OnResourceBufferChanged -= HandleResourceBufferChanged;
    }

    private void HandleResourceBufferChanged(int count)
    {
        RefreshBufferMeshes(count);

        int max = ProductionManager.Instance.ResourceBufferMax;

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
        // MAX UI는 HandleResourceBufferChanged에서 관리 — OnTick은 투입 로직만 담당

        // 자원 버퍼 MAX → 투입 스킵
        if (ProductionManager.Instance.ResourceBuffer >= ProductionManager.Instance.ResourceBufferMax)
        {
            return;
        }

        // 백팩 자원 없음 → 스킵
        if (player.Inventory.BackpackResource <= 0)
        {
            return;
        }

        if (!player.Inventory.ConsumeResource(1))
        {
            return;
        }

        ProductionManager.Instance.AddResource(1);
        SFXManager.Instance?.PlayResourceDrop();

        // FlyObject 도착지 = 방금 켜질 메시 위치 (메시 갱신은 이벤트로 처리)
        PlayFlyEffect(player.FlySocket.position, GetTopMeshPosition());
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

    // 현재 활성 메시 중 가장 위 위치 반환 (FlyObject 도착지)
    private Vector3 GetTopMeshPosition()
    {
        if (_bufferSocket == null)
        {
            return transform.position;
        }

        int nextIndex = _activeMeshes.Count; // 다음에 쌓일 위치
        return _bufferSocket.position + new Vector3(0f, nextIndex * _stackHeight, 0f);
    }

    private void PlayFlyEffect(Vector3 from, Vector3 to)
    {
        ResourceFlyObject fly = _flyPool.Get();
        fly.Fly(from, to, _flyDuration, () => _flyPool.Return(fly));
    }
}
