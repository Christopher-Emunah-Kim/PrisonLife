/// <summary>
/// 감옥 확장 업그레이드 Zone. 완료 시 펜스 교체 → 컷씬(확장 구역 안내) → ExpandCapacity() → 게임 종료 트리거.
/// 소유: 씬 PrisonExpandZone 오브젝트 (OnPrisonFull 후 UpgradeManager가 SetActive(true))
/// 의존: UpgradeZone, CameraController, PrisonManager, GameBalanceData
/// </summary>
/// 수정 로그:
/// 2026-05-17 컷씬 콜백 구조 도입 — SwapFence 즉시 실행, ExpandCapacity는 컷씬 완료 후 콜백에서 호출
using UnityEngine;

public class PrisonExpandZone : UpgradeZone
{
    [SerializeField] private GameBalanceData  _balanceData;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private Transform        _cutsceneTarget;  // UpgradeManager의 _prisonExpandCutsceneTarget과 동일 오브젝트 연결

    [Header("감옥 펜스 가시성")]
    [SerializeField] private GameObject _fenceOriginal;   // PrisonFenceOriginal — 초기 표시
    [SerializeField] private GameObject _fenceExpanded;   // PrisonFenceExpanded — 확장 후 표시

    protected override void InitCost()
    {
        _totalCost = _balanceData.prisonExpandCost;
    }

    protected override void OnUpgradeCompleted()
    {
        // 1. 펜스 교체 + 한도 갱신 + CounterUI 갱신 즉시 처리
        SwapFence();

        if (PrisonManager.Instance != null)
        {
            PrisonManager.Instance.ExpandCapacity();
        }
        else
        {
            Logger.Error("PrisonExpandZone", "PrisonManager.Instance가 null");
        }

        gameObject.SetActive(false);

        // 2. 컷씬 → 완료 콜백에서 OnPrisonExpanded 발행 → GameEndUI
        if (_cameraController == null || _cutsceneTarget == null)
        {
            Logger.Warn("PrisonExpandZone", "컷씬4 스킵 — CameraController 또는 Target 없음");
            NotifyExpanded();
            return;
        }

        _cameraController.PlayCutscene(_cutsceneTarget.position, NotifyExpanded);
    }

    private void NotifyExpanded()
    {
        if (PrisonManager.Instance != null)
        {
            PrisonManager.Instance.NotifyExpanded();
        }
    }

    private void SwapFence()
    {
        if (_fenceOriginal != null)
        {
            _fenceOriginal.SetActive(false);
        }
        else
        {
            Logger.Warn("PrisonExpandZone", "PrisonFenceOriginal 참조 없음");
        }

        if (_fenceExpanded != null)
        {
            _fenceExpanded.SetActive(true);
        }
        else
        {
            Logger.Warn("PrisonExpandZone", "PrisonFenceExpanded 참조 없음");
        }
    }
}
