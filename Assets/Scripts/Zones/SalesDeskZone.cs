/// <summary>
/// 플레이어 쟁반의 완제품 전량을 판매 책상에 내려놓는 Zone.
/// 소유: 씬 내 SalesDeskZone GameObject
/// 의존: InteractionZone, InventoryComponent, GameBalanceData, MaxIndicatorUI, ResourceFlyObject
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class SalesDeskZone : InteractionZone
{
    [Header("데이터")]
    [SerializeField] private GameBalanceData _balanceData;

    [Header("버퍼 메시 적층")]
    [SerializeField] private StackMeshItem _deskMeshPrefab;
    [SerializeField] private Transform     _deskSocket;
    [SerializeField] private float         _stackHeight = 0.2f;
    [SerializeField] private int           _meshUnit    = 5;
    [SerializeField] private int           _poolSize    = 10;

    [Header("연출")]
    [SerializeField] private ResourceFlyObject _flyPrefab;
    [SerializeField] private int               _flyPoolSize = 10;
    [SerializeField] private float             _flyDuration = 0.4f;

    [Header("UI")]
    [SerializeField] private MaxIndicatorUI _maxIndicatorUI;

    private ObjectPool<StackMeshItem>     _deskPool;
    private ObjectPool<ResourceFlyObject> _flyPool;
    private readonly List<StackMeshItem>  _activeMeshes = new List<StackMeshItem>();

    private int _deskGoods;  // 현재 책상 위 완제품 수

    public int DeskGoods    => _deskGoods;
    public int SalesDeskMax => _balanceData != null ? _balanceData.salesDeskMax : 0;

    private void Awake()
    {
        _deskPool = new ObjectPool<StackMeshItem>(_deskMeshPrefab, _poolSize, transform);
        _flyPool  = new ObjectPool<ResourceFlyObject>(_flyPrefab, _flyPoolSize, transform);
    }

    // SalesZone이 판매 틱마다 책상 재고 차감 시 호출
    public bool ConsumeFromDesk(int amount)
    {
        if (_deskGoods < amount)
        {
            return false;
        }

        _deskGoods -= amount;
        RefreshDeskMeshes();
        UpdateMaxUI();
        return true;
    }

    protected override void OnTick(PlayerCharacter player)
    {
        // 책상 MAX → 적재 스킵
        if (_deskGoods >= SalesDeskMax)
        {
            return;
        }

        // 쟁반에 완제품 없음 → 스킵
        if (player.Inventory.TrayGoods <= 0)
        {
            return;
        }

        if (!player.Inventory.ConsumeGoods(1))
        {
            return;
        }

        _deskGoods++;
        RefreshDeskMeshes();
        UpdateMaxUI();

        PlayFlyEffect(player.FlySocket.position, GetTopMeshPosition());
    }

    private void RefreshDeskMeshes()
    {
        if (_deskSocket == null)
        {
            return;
        }

        int targetCount = _deskGoods / _meshUnit;

        while (_activeMeshes.Count < targetCount)
        {
            StackMeshItem item = _deskPool.Get();
            item.transform.SetParent(_deskSocket, false);
            _activeMeshes.Add(item);
        }

        while (_activeMeshes.Count > targetCount)
        {
            int last = _activeMeshes.Count - 1;
            _deskPool.Return(_activeMeshes[last]);
            _activeMeshes.RemoveAt(last);
        }

        for (int i = 0; i < _activeMeshes.Count; i++)
        {
            _activeMeshes[i].transform.localPosition = new Vector3(0f, i * _stackHeight, 0f);
        }
    }

    private Vector3 GetTopMeshPosition()
    {
        if (_deskSocket == null)
        {
            return transform.position;
        }

        int nextIndex = _activeMeshes.Count;
        return _deskSocket.position + new Vector3(0f, nextIndex * _stackHeight, 0f);
    }

    private void UpdateMaxUI()
    {
        if (_deskGoods >= SalesDeskMax)
        {
            _maxIndicatorUI?.SetVisible(true);
        }
        else
        {
            _maxIndicatorUI?.SetVisible(false);
        }
    }

    private void PlayFlyEffect(Vector3 from, Vector3 to)
    {
        ResourceFlyObject fly = _flyPool.Get();
        fly.Fly(from, to, _flyDuration, () => _flyPool.Return(fly));
    }
}
