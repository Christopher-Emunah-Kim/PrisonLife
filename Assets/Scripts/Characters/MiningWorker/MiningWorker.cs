/// <summary>
/// 채굴 인부 FSM. Moving(목표 셀로 이동) → Mining(채굴 틱) 두 상태를 순환.
/// 소유: MiningWorkerHireZone (스폰), 씬 MiningWorker 오브젝트들
/// 의존: BaseCharacter, MiningGrid, GridCell, ResourceFlyObject, ProductionManager, GameBalanceData
/// </summary>
/// 수정 로그:
/// 2026-05-17 WaitForSeconds 캐싱 (_miningTickWait, _retryWait)
/// 2026-05-17 씬 참조(MiningGrid, ResourceDropTarget, GameBalanceData) Awake에서 자동 탐색으로 변경
using System.Collections;
using UnityEngine;

public class MiningWorker : BaseCharacter
{
    public enum MiningFSMState { Moving, Mining }

    [Header("연출 — 프리팹 전용 슬롯")]
    [SerializeField] private ResourceFlyObject _flyPrefab;

    [Header("채굴")]
    [SerializeField] private float _miningTickInterval = 0.5f;   // 채굴 1회 소요 시간
    [SerializeField] private float _arrivalThreshold   = 0.15f;  // 셀 도착 판정 거리

    [Header("연출")]
    [SerializeField] private float _flyDuration = 0.4f;
    [SerializeField] private int   _flyPoolSize  = 5;

    // 씬 참조 — Awake에서 자동 탐색
    private MiningGrid  _miningGrid;
    private GameBalanceData _balanceData;
    private Transform   _resourceDropTarget;

    private MiningFSMState _currentState = MiningFSMState.Moving;
    private GridCell       _targetCell;
    private Coroutine      _miningCoroutine;
    private ObjectPool<ResourceFlyObject> _flyPool;

    // 버퍼 MAX 여부 캐시 — OnResourceBufferChanged 이벤트로 갱신
    private bool _isBufferFull;

    private WaitForSeconds _miningTickWait;
    private WaitForSeconds _retryWait;

    // ── 초기화 ────────────────────────────────────────────

    private void Awake()
    {
        // 씬 참조 자동 탐색 (GameBalanceData는 MiningWorkerHireZone이 Init()으로 주입)
        _miningGrid = FindObjectOfType<MiningGrid>();

        ResourceDropZone dropZone = FindObjectOfType<ResourceDropZone>();
        if (dropZone != null)
        {
            _resourceDropTarget = dropZone.transform;
        }

        if (_miningGrid == null)
        {
            Logger.Error("MiningWorker", "MiningGrid를 씬에서 찾지 못함");
        }

        if (_resourceDropTarget == null)
        {
            Logger.Error("MiningWorker", "ResourceDropZone을 씬에서 찾지 못함");
        }

        _flyPool        = new ObjectPool<ResourceFlyObject>(_flyPrefab, _flyPoolSize, transform);
        _miningTickWait = new WaitForSeconds(_miningTickInterval);
    }

    /// <summary>MiningWorkerHireZone이 Instantiate 직후 호출 — GameBalanceData 주입.</summary>
    public void Init(GameBalanceData balanceData)
    {
        _balanceData = balanceData;
        _retryWait   = new WaitForSeconds(_balanceData.miningRegenTime);
    }

    private void OnDestroy()
    {
        if (ProductionManager.Instance != null)
        {
            ProductionManager.Instance.OnResourceBufferChanged -= HandleResourceBufferChanged;
        }

        StopMining();
    }

    private void Start()
    {
        if (ProductionManager.Instance != null)
        {
            ProductionManager.Instance.OnResourceBufferChanged += HandleResourceBufferChanged;
            _isBufferFull = ProductionManager.Instance.ResourceBuffer >= ProductionManager.Instance.ResourceBufferMax;
        }

        TransitionToMoving();
    }

    // ── FSM 업데이트 ─────────────────────────────────────

    private void Update()
    {
        if (_currentState != MiningFSMState.Moving)
        {
            return;
        }

        UpdateMoving();
    }

    // ── Moving 상태 ───────────────────────────────────────

    private void TransitionToMoving()
    {
        _currentState = MiningFSMState.Moving;
        StopMining();
        FindAndReserveNextCell();
    }

    private void FindAndReserveNextCell()
    {
        if (_miningGrid == null)
        {
            Logger.Warn("MiningWorker", "MiningGrid 참조 없음 — 이동 중단");
            return;
        }

        // 기존 예약 해제 후 새 셀 탐색
        ReleaseCurrentCell();

        GridCell next = _miningGrid.GetNearestMinableCell(transform.position);

        if (next == null)
        {
            // 채굴 가능 셀 없음 — 잠시 후 재탐색
            _miningCoroutine = StartCoroutine(WaitAndRetryMoving());
            return;
        }

        next.Reserve();
        _targetCell = next;
    }

    private void UpdateMoving()
    {
        if (_targetCell == null)
        {
            return;
        }

        // 예약 셀이 이미 채굴됐으면 재탐색
        if (!_targetCell.IsMinable)
        {
            FindAndReserveNextCell();
            return;
        }

        Vector3 targetPos  = _targetCell.transform.position;
        Vector3 flatTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        Vector3 direction  = flatTarget - transform.position;

        if (direction.magnitude <= _arrivalThreshold)
        {
            // 도착 → Mining 전환
            TransitionToMining();
            return;
        }

        Move(direction.normalized);
    }

    // ── Mining 상태 ───────────────────────────────────────

    private void TransitionToMining()
    {
        _currentState = MiningFSMState.Mining;
        _miningCoroutine = StartCoroutine(MiningRoutine());
    }

    private IEnumerator MiningRoutine()
    {
        yield return _miningTickWait;

        if (_targetCell == null || !_targetCell.IsMinable)
        {
            // 셀이 사라졌으면 다시 Moving
            TransitionToMoving();
            yield break;
        }

        // 셀 채굴
        _miningGrid.MineCell(_targetCell);
        _targetCell = null;

        // 자원 날아가는 연출 + 버퍼 추가 (버퍼 MAX면 연출만)
        PlayFlyEffect();

        if (!_isBufferFull)
        {
            ProductionManager.Instance?.AddResource(1);
        }

        _miningCoroutine = null;
        TransitionToMoving();
    }

    // ── 헬퍼 ──────────────────────────────────────────────

    private IEnumerator WaitAndRetryMoving()
    {
        // Init() 전에 호출되면 _retryWait가 null — fallback
        if (_retryWait == null)
        {
            Logger.Warn("MiningWorker", "GameBalanceData 미주입 — 재탐색 대기 3f 기본값 사용");
            yield return new WaitForSeconds(3f);
        }
        else
        {
            yield return _retryWait;
        }
        _miningCoroutine = null;
        FindAndReserveNextCell();
    }

    private void StopMining()
    {
        if (_miningCoroutine != null)
        {
            StopCoroutine(_miningCoroutine);
            _miningCoroutine = null;
        }
    }

    private void ReleaseCurrentCell()
    {
        if (_targetCell != null)
        {
            _targetCell.CancelReserve();
            _targetCell = null;
        }
    }

    private void PlayFlyEffect()
    {
        if (_flyPrefab == null || _resourceDropTarget == null)
        {
            return;
        }

        ResourceFlyObject fly = _flyPool.Get();
        fly.Fly(transform.position, _resourceDropTarget.position, _flyDuration, () => _flyPool.Return(fly));
    }

    private void HandleResourceBufferChanged(int count)
    {
        if (ProductionManager.Instance == null)
        {
            return;
        }

        _isBufferFull = count >= ProductionManager.Instance.ResourceBufferMax;
    }
}
