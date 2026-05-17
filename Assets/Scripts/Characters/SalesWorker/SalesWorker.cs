/// <summary>
/// 판매 인부 FSM. WaitingAtSales → MovingToPickup → MovingToDesk → MovingToSales 루프.
/// 책상 완제품 소진 시 완제품 버퍼로 픽업 → 책상 적재 → 판매 트리거 대기 순환.
/// 소유: SalesWorkerHireZone (스폰), 씬 SalesWorker 오브젝트
/// 의존: BaseCharacter, GoodsPickupZone, SalesDeskZone, SalesZone, GameBalanceData
/// </summary>
/// 수정 로그:
/// 2026-05-17 InventoryComponent 제거 — 쟁반 슬롯을 내부 필드로 직접 관리
/// 2026-05-17 쟁반 메시 적층 연출 추가 — StackMeshItem 풀 + 쟁반 소켓
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalesWorker : BaseCharacter
{
    public enum SalesFSMState { WaitingAtSales, MovingToPickup, PickingUp, MovingToDesk, DroppingOff, MovingToSales }

    [Header("연출 — 프리팹 전용 슬롯")]
    [SerializeField] private ResourceFlyObject _flyPrefab;
    [SerializeField] private float             _flyDuration = 0.4f;
    [SerializeField] private int               _flyPoolSize  = 5;

    [Header("쟁반 메시 적층")]
    [SerializeField] private StackMeshItem _goodsMeshPrefab;
    [SerializeField] private Transform     _traySocket;       // 쟁반 위 메시 쌓일 기준점
    [SerializeField] private float         _stackHeight = 0.2f;
    [SerializeField] private int           _meshUnit    = 5;
    [SerializeField] private int           _meshPoolSize = 8;

    [Header("이동")]
    [SerializeField] private float _arrivalThreshold = 0.3f;

    // 씬 참조 — Awake에서 자동 탐색, GameBalanceData는 Init()으로 주입
    private GoodsPickupZone _pickupZone;
    private SalesDeskZone   _deskZone;
    private SalesZone       _salesZone;
    private GameBalanceData _balanceData;

    // 쟁반 슬롯 — InventoryComponent 없이 직접 관리
    private int _trayGoods;
    private int _trayMax;

    // 쟁반 메시 풀 + 활성 목록
    private ObjectPool<StackMeshItem>    _meshPool;
    private readonly List<StackMeshItem> _activeMeshes = new List<StackMeshItem>();

    private ObjectPool<ResourceFlyObject> _flyPool;
    private SalesFSMState  _currentState = SalesFSMState.WaitingAtSales;
    private Coroutine      _actionCoroutine;
    private WaitForSeconds _tickWait;

    // ── 초기화 ────────────────────────────────────────────

    private void Awake()
    {
        _pickupZone = FindObjectOfType<GoodsPickupZone>();
        _deskZone   = FindObjectOfType<SalesDeskZone>();
        _salesZone  = FindObjectOfType<SalesZone>();

        if (_pickupZone == null) { Logger.Error("SalesWorker", "GoodsPickupZone을 씬에서 찾지 못함"); }
        if (_deskZone   == null) { Logger.Error("SalesWorker", "SalesDeskZone을 씬에서 찾지 못함"); }
        if (_salesZone  == null) { Logger.Error("SalesWorker", "SalesZone을 씬에서 찾지 못함"); }

        _flyPool  = new ObjectPool<ResourceFlyObject>(_flyPrefab, _flyPoolSize, transform);
        _meshPool = new ObjectPool<StackMeshItem>(_goodsMeshPrefab, _meshPoolSize, transform);
        _tickWait = new WaitForSeconds(0.1f);
    }

    private void OnDestroy()
    {
        if (ProductionManager.Instance != null)
        {
            ProductionManager.Instance.OnGoodsBufferChanged -= HandleGoodsBufferChanged;
        }

        if (_deskZone != null)
        {
            _deskZone.OnDeskEmpty -= HandleDeskEmpty;
        }

        StopAction();

        if (_currentState == SalesFSMState.WaitingAtSales)
        {
            _salesZone?.OnWorkerExit();
        }
    }

    /// <summary>SalesWorkerHireZone이 Instantiate 직후 호출 — 초기화 + 이벤트 구독 + FSM 시작.</summary>
    public void Init(GameBalanceData balanceData)
    {
        _balanceData = balanceData;
        _trayMax     = balanceData.GetMiningLevel(0).backpackMax;

        if (ProductionManager.Instance != null)
        {
            ProductionManager.Instance.OnGoodsBufferChanged += HandleGoodsBufferChanged;
        }

        if (_deskZone != null)
        {
            _deskZone.OnDeskEmpty += HandleDeskEmpty;
        }

        // 스폰 위치에서 SalesZone으로 먼저 이동 후 대기 시작
        TransitionTo(SalesFSMState.MovingToSales);
    }

    // ── FSM ──────────────────────────────────────────────

    private void Update()
    {
        switch (_currentState)
        {
            case SalesFSMState.MovingToPickup:
                MoveToward(_pickupZone.transform.position, OnArrivedAtPickup);
                break;
            case SalesFSMState.MovingToDesk:
                MoveToward(_deskZone.transform.position, OnArrivedAtDesk);
                break;
            case SalesFSMState.MovingToSales:
                MoveToward(_salesZone.transform.position, OnArrivedAtSales);
                break;
            // WaitingAtSales / PickingUp / DroppingOff: 이동 없음
        }
    }

    private void TransitionTo(SalesFSMState next)
    {
        StopAction();
        _currentState = next;

        switch (next)
        {
            case SalesFSMState.WaitingAtSales:
                _salesZone?.OnWorkerEnter();
                CheckDeskAndDecide();
                break;
            case SalesFSMState.MovingToPickup:
                _salesZone?.OnWorkerExit();
                break;
        }
    }

    // ── 이동 헬퍼 ─────────────────────────────────────

    private void MoveToward(Vector3 destination, System.Action onArrived)
    {
        Vector3 flatDest = new Vector3(destination.x, transform.position.y, destination.z);
        Vector3 dir      = flatDest - transform.position;

        if (dir.magnitude <= _arrivalThreshold)
        {
            onArrived?.Invoke();
            return;
        }

        Move(dir.normalized);
    }

    // ── 도착 콜백 ─────────────────────────────────────

    private void OnArrivedAtPickup()
    {
        _currentState    = SalesFSMState.PickingUp;
        _actionCoroutine = StartCoroutine(PickupRoutine());
    }

    private void OnArrivedAtDesk()
    {
        _currentState    = SalesFSMState.DroppingOff;
        _actionCoroutine = StartCoroutine(DropRoutine());
    }

    private void OnArrivedAtSales()
    {
        TransitionTo(SalesFSMState.WaitingAtSales);
    }

    // ── 픽업 Coroutine ────────────────────────────────

    private IEnumerator PickupRoutine()
    {
        // 버퍼에 완제품이 생길 때까지 대기
        while (ProductionManager.Instance == null || ProductionManager.Instance.GoodsBuffer <= 0)
        {
            yield return _tickWait;
        }

        // 쟁반이 찰 때까지 픽업
        while (_trayGoods < _trayMax)
        {
            if (ProductionManager.Instance.GoodsBuffer <= 0)
            {
                break;
            }

            if (!ProductionManager.Instance.ConsumeGoods(1))
            {
                break;
            }

            _trayGoods++;
            RefreshTrayMeshes();
            PlayFlyEffect(_pickupZone.transform.position, transform.position);
            ProductionManager.Instance.TryResumeProduction();

            yield return _tickWait;
        }

        _actionCoroutine = null;
        TransitionTo(SalesFSMState.MovingToDesk);
    }

    // ── 적재 Coroutine ────────────────────────────────

    private IEnumerator DropRoutine()
    {
        while (_trayGoods > 0)
        {
            if (_deskZone.DeskGoods >= _deskZone.SalesDeskMax)
            {
                break;
            }

            _trayGoods--;
            RefreshTrayMeshes();
            _deskZone.AddGoodsFromWorker(1);
            PlayFlyEffect(transform.position, _deskZone.transform.position);

            yield return _tickWait;
        }

        _actionCoroutine = null;
        TransitionTo(SalesFSMState.MovingToSales);
    }

    // ── 책상 소진 감지 ────────────────────────────────

    private void CheckDeskAndDecide()
    {
        if (_deskZone == null)
        {
            return;
        }

        if (_deskZone.DeskGoods <= 0)
        {
            TransitionTo(SalesFSMState.MovingToPickup);
        }
    }

    private void HandleDeskEmpty()
    {
        if (_currentState != SalesFSMState.WaitingAtSales)
        {
            return;
        }

        // 버퍼에 완제품이 있으면 즉시 픽업 출발, 없으면 버퍼 이벤트 대기
        if (ProductionManager.Instance != null && ProductionManager.Instance.GoodsBuffer > 0)
        {
            TransitionTo(SalesFSMState.MovingToPickup);
        }
    }

    private void HandleGoodsBufferChanged(int count)
    {
        if (_currentState != SalesFSMState.WaitingAtSales)
        {
            return;
        }

        if (_deskZone == null || _deskZone.DeskGoods > 0)
        {
            return;
        }

        if (count > 0)
        {
            TransitionTo(SalesFSMState.MovingToPickup);
        }
    }

    // ── 헬퍼 ──────────────────────────────────────────

    private void RefreshTrayMeshes()
    {
        if (_traySocket == null || _goodsMeshPrefab == null)
        {
            return;
        }

        int targetCount = _trayGoods / _meshUnit;

        while (_activeMeshes.Count < targetCount)
        {
            StackMeshItem item = _meshPool.Get();
            item.transform.SetParent(_traySocket, false);
            _activeMeshes.Add(item);
        }

        while (_activeMeshes.Count > targetCount)
        {
            int last = _activeMeshes.Count - 1;
            _meshPool.Return(_activeMeshes[last]);
            _activeMeshes.RemoveAt(last);
        }

        for (int i = 0; i < _activeMeshes.Count; i++)
        {
            _activeMeshes[i].transform.localPosition = new Vector3(0f, i * _stackHeight, 0f);
        }
    }

    private void StopAction()
    {
        if (_actionCoroutine != null)
        {
            StopCoroutine(_actionCoroutine);
            _actionCoroutine = null;
        }
    }

    private void PlayFlyEffect(Vector3 from, Vector3 to)
    {
        if (_flyPrefab == null)
        {
            return;
        }

        ResourceFlyObject fly = _flyPool.Get();
        fly.Fly(from, to, _flyDuration, () => _flyPool.Return(fly));
    }
}
