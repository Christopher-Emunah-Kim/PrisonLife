/// <summary>
/// 죄수 ObjectPool 관리 + 스폰 Coroutine. 큐 MAX(5) 도달 시 스폰 중단.
/// 소유: 씬 PrisonerSpawner GameObject
/// 의존: ObjectPool<Prisoner>, PrisonerData, GameBalanceData, SalesZone
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerSpawner : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private PrisonerData    _prisonerData;
    [SerializeField] private GameBalanceData _balanceData;

    [Header("풀링")]
    [SerializeField] private Prisoner _prisonerPrefab;
    [SerializeField] private int      _poolSize = 30;

    [Header("위치")]
    [SerializeField] private Transform   _spawnPoint;       // 9시 방향 끝 스폰 위치
    [SerializeField] private Transform[] _queueSlots;       // 판매 Zone 앞 대기 슬롯 (최대 5개)

    private ObjectPool<Prisoner>  _pool;
    // 현재 대기 중인 죄수 큐 (앞 = [0])
    private readonly List<Prisoner> _waitingQueue   = new List<Prisoner>();
    // 판매 완료 후 감옥으로 이동 중인 죄수 목록
    private readonly List<Prisoner> _movingToPrison = new List<Prisoner>();

    public int  QueueCount    => _waitingQueue.Count;
    public bool IsQueueFull   => _waitingQueue.Count >= (_prisonerData != null ? _prisonerData.maxQueueSize : 5);
    // 큐 맨 앞 죄수
    public Prisoner FrontPrisoner => _waitingQueue.Count > 0 ? _waitingQueue[0] : null;

    private Coroutine _spawnCoroutine;

    private void Awake()
    {
        _pool = new ObjectPool<Prisoner>(_prisonerPrefab, _poolSize, transform);
    }

    private void Start()
    {
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDestroy()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_prisonerData != null ? _prisonerData.spawnInterval : 3f);

            if (IsQueueFull)
            {
                continue;
            }

            SpawnPrisoner();
        }
    }

    private void SpawnPrisoner()
    {
        if (_prisonerData == null || _balanceData == null)
        {
            Logger.Warn("PrisonerSpawner", "PrisonerData 또는 GameBalanceData 미설정");
            return;
        }

        int queueIndex = _waitingQueue.Count;
        if (_queueSlots == null || queueIndex >= _queueSlots.Length)
        {
            Logger.Warn("PrisonerSpawner", $"QueueSlot 인덱스 초과: {queueIndex}");
            return;
        }

        Prisoner prisoner = _pool.Get();
        prisoner.transform.position = _spawnPoint != null ? _spawnPoint.position : transform.position;

        int purchaseCount = Random.Range(_prisonerData.purchaseMin, _prisonerData.purchaseMax + 1);
        prisoner.Setup(purchaseCount, _balanceData.goodsPrice, _queueSlots[queueIndex].position);

        _waitingQueue.Add(prisoner);

        // 큐 도착 감지 Coroutine 시작
        StartCoroutine(WaitForQueueArrival(prisoner, queueIndex));
    }

    private IEnumerator WaitForQueueArrival(Prisoner prisoner, int slotIndex)
    {
        // 목적지에 도착할 때까지 대기
        while (prisoner != null && prisoner.gameObject.activeSelf)
        {
            float dist = Vector3.Distance(prisoner.transform.position, _queueSlots[slotIndex].position);
            if (dist < 0.3f)
            {
                prisoner.OnReachedQueue();
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 판매 완료 시 SalesZone이 호출. 큐 맨 앞 죄수를 감옥 목적지로 이동 후 풀 반환 예약.
    /// </summary>
    /// <param name="prisonTargetPosition">감옥 입구 월드 위치</param>
    public void DequeueFront(Vector3 prisonTargetPosition)
    {
        if (_waitingQueue.Count == 0)
        {
            Logger.Warn("PrisonerSpawner", "DequeueFront: 큐가 비어있음");
            return;
        }

        Prisoner front = _waitingQueue[0];
        _waitingQueue.RemoveAt(0);

        // 남은 죄수들 슬롯 재배치
        ShiftQueue();

        // 감옥 목적지로 직선 이동 시작
        _movingToPrison.Add(front);
        front.MoveTowardPrison(prisonTargetPosition);

        // 이미 감옥이 만원 상태면 이동 시작 직후 No Cell 표시
        if (PrisonManager.Instance != null && PrisonManager.Instance.IsFull)
        {
            front.ShowNoCellBubble(true);
        }
    }

    /// <summary>
    /// PrisonZone이 죄수를 수용할 때 호출 — 풀에 반환.
    /// </summary>
    public void ReturnToPool(Prisoner prisoner)
    {
        _movingToPrison.Remove(prisoner);
        _pool.Return(prisoner);
    }

    // 앞 죄수 빠진 후 나머지 슬롯 위치 재배치
    private void ShiftQueue()
    {
        for (int i = 0; i < _waitingQueue.Count; i++)
        {
            if (_queueSlots != null && i < _queueSlots.Length)
            {
                _waitingQueue[i].SetMoveTarget(_queueSlots[i].position);
            }
        }
    }

    /// <summary>
    /// "No Cell!" 상태 업데이트 — 큐 내 모든 죄수에 적용.
    /// </summary>
    public void SetNoCellAll(bool show)
    {
        foreach (Prisoner queued in _waitingQueue)
        {
            if (queued != null)
            {
                queued.ShowNoCellBubble(show);
            }
        }

        foreach (Prisoner moving in _movingToPrison)
        {
            if (moving != null)
            {
                moving.ShowNoCellBubble(show);
            }
        }
    }
}
