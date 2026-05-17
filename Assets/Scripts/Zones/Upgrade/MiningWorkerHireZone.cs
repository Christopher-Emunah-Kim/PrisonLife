/// <summary>
/// 채굴 인부 고용 Zone. 완료 시 MiningWorker 3명을 지정 위치에 스폰.
/// 소유: 씬 MiningWorkerHireZone 오브젝트
/// 의존: UpgradeZone, GameBalanceData
/// </summary>
using UnityEngine;

public class MiningWorkerHireZone : UpgradeZone
{
    [SerializeField] private GameBalanceData _balanceData;

    // MiningWorker Prefab (MODULE-11 구현 전까지 null 허용)
    [SerializeField] private GameObject   _workerPrefab;
    [SerializeField] private Transform[]  _spawnPoints;  // 3개 위치 지정

    protected override void InitCost()
    {
        _totalCost = _balanceData.miningWorkerCost;
    }

    protected override void OnUpgradeCompleted()
    {
        SpawnWorkers();
        gameObject.SetActive(false);
    }

    private void SpawnWorkers()
    {
        if (_workerPrefab == null)
        {
            Logger.Warn("MiningWorkerHireZone", "MiningWorker 프리팹 미연결 — 스폰 생략");
            return;
        }

        int spawnCount = (_spawnPoints != null && _spawnPoints.Length > 0)
            ? Mathf.Min(3, _spawnPoints.Length)
            : 3;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = (_spawnPoints != null && i < _spawnPoints.Length && _spawnPoints[i] != null)
                ? _spawnPoints[i].position
                : transform.position;

            GameObject obj    = Instantiate(_workerPrefab, pos, Quaternion.identity);
            MiningWorker worker = obj.GetComponent<MiningWorker>();
            if (worker != null)
            {
                worker.Init(_balanceData);
            }
        }

        Logger.Log("MiningWorkerHireZone", $"MiningWorker {spawnCount}명 스폰 완료");
    }
}
