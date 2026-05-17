/// <summary>
/// 판매 인부 고용 Zone. 완료 시 SalesWorker 1명을 지정 위치에 스폰.
/// 소유: 씬 SalesWorkerHireZone 오브젝트
/// 의존: UpgradeZone, GameBalanceData
/// </summary>
using UnityEngine;

public class SalesWorkerHireZone : UpgradeZone
{
    [SerializeField] private GameBalanceData _balanceData;

    // SalesWorker Prefab (MODULE-12 구현 전까지 null 허용)
    [SerializeField] private GameObject _workerPrefab;
    [SerializeField] private Transform  _spawnPoint;

    protected override void InitCost()
    {
        _totalCost = _balanceData.salesWorkerCost;
    }

    protected override void OnUpgradeCompleted()
    {
        SpawnWorker();
        gameObject.SetActive(false);
    }

    private void SpawnWorker()
    {
        if (_workerPrefab == null)
        {
            Logger.Warn("SalesWorkerHireZone", "SalesWorker 프리팹 미연결 — 스폰 생략");
            return;
        }

        Vector3 pos = (_spawnPoint != null) ? _spawnPoint.position : transform.position;
        Instantiate(_workerPrefab, pos, Quaternion.identity);

        Logger.Log("SalesWorkerHireZone", "SalesWorker 1명 스폰 완료");
    }
}
