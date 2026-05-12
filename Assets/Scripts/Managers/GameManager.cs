/// <summary>
/// 씬 전체 초기화 및 게임 종료 흐름을 제어하는 최상위 관리자.
/// 소유: 씬 루트 (단일 인스턴스)
/// 의존: SalesManager, PrisonManager
/// </summary>
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action OnGameEnded;

    public static GameManager Instance { get; private set; }

    // DD3: OnFirstSaleCompleted 시 동시 활성화할 UpgradeZone 3종
    [SerializeField] private GameObject _drillUpgradeZone;
    [SerializeField] private GameObject _miningWorkerHireZone;
    [SerializeField] private GameObject _salesWorkerHireZone;

    // DD3: OnPrisonFull 시 활성화할 UpgradeZone
    [SerializeField] private GameObject _prisonExpandZone;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        SalesManager.OnFirstSaleCompleted += HandleFirstSaleCompleted;
        PrisonManager.OnPrisonFull        += HandlePrisonFull;
        PrisonManager.OnPrisonExpanded    += HandlePrisonExpanded;
    }

    private void OnDisable()
    {
        SalesManager.OnFirstSaleCompleted -= HandleFirstSaleCompleted;
        PrisonManager.OnPrisonFull        -= HandlePrisonFull;
        PrisonManager.OnPrisonExpanded    -= HandlePrisonExpanded;
    }

    // DD3: 첫 판매 완료 → 드릴/채굴인부/판매인부 Zone 동시 활성화
    private void HandleFirstSaleCompleted()
    {
        if (_drillUpgradeZone != null)
        {
            _drillUpgradeZone.SetActive(true);
        }
        if (_miningWorkerHireZone != null)
        {
            _miningWorkerHireZone.SetActive(true);
        }
        if (_salesWorkerHireZone != null)
        {
            _salesWorkerHireZone.SetActive(true);
        }
    }

    // 감옥 만원 → PrisonExpandZone 활성화
    private void HandlePrisonFull()
    {
        if (_prisonExpandZone != null)
        {
            _prisonExpandZone.SetActive(true);
        }
    }

    // 감옥 확장 완료 → 게임 종료
    private void HandlePrisonExpanded()
    {
        TriggerGameEnd();
    }

    public void TriggerGameEnd()
    {
        Logger.Log("GameManager", "Game End");
        Time.timeScale = 0f;
        OnGameEnded?.Invoke();
    }
}
