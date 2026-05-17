/// <summary>
/// 8×16 채굴 그리드 관리. 셀 채굴 처리 + 리젠 Coroutine + 가장 가까운 셀 탐색.
/// 소유: 씬 MiningGrid 오브젝트
/// 의존: GridCell, GameBalanceData
/// </summary>
/// 수정 로그:
/// 2026-05-17 WaitForSeconds 캐싱 (_regenPollWait)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningGrid : MonoBehaviour
{
    [SerializeField] private GameBalanceData _balanceData;

    // Inspector에서 8×16 GridCell 오브젝트 배열 연결
    [SerializeField] private GridCell[] _cells;

    // 리젠 폴링 간격
    [SerializeField] private float _regenPollInterval = 0.5f;

    // 에디터 Grid Generator 전용 (런타임 미사용)
    [SerializeField] private float      _cellSpacing = 1f;
    [SerializeField] private GameObject _cellPrefab;

    // 리젠 대기 중인 셀 큐 (셀, 리젠 완료 시각)
    private readonly Queue<(GridCell cell, float regenAt)> _regenQueue = new();
    private WaitForSeconds _regenPollWait;

    private void Awake()
    {
        _regenPollWait = new WaitForSeconds(_regenPollInterval);
    }

    private void Start()
    {
        StartCoroutine(RegenRoutine());
    }

    // ── 외부 API ─────────────────────────────────────────

    /// <summary>셀 채굴 후 리젠 큐에 등록.</summary>
    public void MineCell(GridCell cell)
    {
        if (cell == null || !cell.IsMinable)
        {
            return;
        }

        cell.Mine();

        if (_balanceData == null)
        {
            Logger.Error("MiningGrid", "GameBalanceData가 null — MineCell 중단");
            return;
        }

        float regenTime = _balanceData.miningRegenTime;
        _regenQueue.Enqueue((cell, Time.time + regenTime));
    }

    /// <summary>미예약 채굴 가능 셀 중 requester에서 가장 가까운 셀 반환. 없으면 null.</summary>
    public GridCell GetNearestMinableCell(Vector3 requesterPos)
    {
        GridCell nearest  = null;
        float    minDist  = float.MaxValue;

        foreach (GridCell cell in _cells)
        {
            if (cell == null || !cell.IsMinable || cell.IsReserved)
            {
                continue;
            }

            float dist = Vector3.SqrMagnitude(cell.transform.position - requesterPos);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = cell;
            }
        }

        return nearest;
    }

    // ── 리젠 Coroutine ────────────────────────────────────

    private IEnumerator RegenRoutine()
    {
        while (true)
        {
            yield return _regenPollWait;

            while (_regenQueue.Count > 0 && _regenQueue.Peek().regenAt <= Time.time)
            {
                var (cell, _) = _regenQueue.Dequeue();

                if (cell != null)
                {
                    cell.Regenerate();
                }
            }
        }
    }
}
