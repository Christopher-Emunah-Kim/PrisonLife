/// <summary>
/// 씬 전체 초기화 및 게임 종료 흐름을 제어하는 최상위 관리자.
/// 업그레이드 Zone 활성화 제어는 UpgradeManager가 전담.
/// 소유: 씬 루트 (단일 인스턴스)
/// 의존: PrisonManager
/// </summary>
/// 수정 로그:
/// 2026-05-17 업그레이드 Zone 레퍼런스를 UpgradeManager로 이전
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action OnGameEnded;

    public static GameManager Instance { get; private set; }

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
        PrisonManager.OnPrisonExpanded += HandlePrisonExpanded;
    }

    private void OnDisable()
    {
        PrisonManager.OnPrisonExpanded -= HandlePrisonExpanded;
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
