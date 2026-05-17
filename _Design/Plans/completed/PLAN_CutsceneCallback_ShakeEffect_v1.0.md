# PLAN_CutsceneCallback_ShakeEffect_v1.0
```yaml
date:    2026-05-17
status:  ACTIVE
designs: [01_Overview.md, 07_Upgrade.md, 08_UI.md]
```

## GOAL
> 컷씬 완료 후 Zone 활성화 순서 보장 (CameraController 콜백 구조) + 생산 라인/감옥 진동 연출 추가.

## SCOPE
```yaml
new_files:
  - Assets/Scripts/Util/ShakeEffect.cs
modified_files:
  - Assets/Scripts/Managers/CameraController.cs
  - Assets/Scripts/Managers/UpgradeManager.cs
  - Assets/Scripts/Managers/ProductionManager.cs
  - Assets/Scripts/Zones/Prison/PrisonZone.cs
  - Assets/Scripts/Zones/Upgrade/PrisonExpandZone.cs
```

## INTEGRATION_POINTS
```yaml
owner:       UpgradeManager (컷씬 + Zone 활성화 타이밍 소유권)
entry:       HandleFirstSaleCompleted / HandlePrisonFull / NotifyDrillCompleted
depends_on:  CameraController, SalesManager, PrisonManager, ShakeEffect
ref_pattern: 기존 CutsceneRoutine 구조 재사용 — onComplete: System.Action 파라미터 추가
arch_impact: |
  CLASS_REGISTRY: ShakeEffect 신규 등록
  INTEGRATION_MAP: UpgradeManager → CameraController.PlayCutscene() 직접 호출 추가
                   CameraController ← SalesManager 이벤트 구독 제거
                   CameraController ← PrisonManager 이벤트 구독 제거
  PATTERNS: "컷씬 완료 후 콜백(Action onComplete)" 패턴 추가
```

## FLOW
```
[컷씬1: OnFirstSaleCompleted]
  UpgradeManager.HandleFirstSaleCompleted()
    │
    ▼
  _cameraController.PlayCutscene(_drillCutsceneTarget.position, OnFirstSaleCutsceneDone)
    │
    ▼ (CutsceneRoutine: lerp → hold → return)
  onComplete?.Invoke()
    │
    ▼
  OnFirstSaleCutsceneDone()
    ├─ DrillUpgradeZone.SetActive(true)
    └─ SalesWorkerHireZone.SetActive(true)

[컷씬2: NotifyDrillCompleted]
  UpgradeManager.NotifyDrillCompleted()
    │
    ▼
  _cameraController.PlayCutscene(_miningWorkerCutsceneTarget.position, OnDrillCutsceneDone)
    │
    ▼
  OnDrillCutsceneDone()
    ├─ DrillUpgradeZone.SetActive(false)
    ├─ TractorUpgradeZone.SetActive(true)
    └─ MiningWorkerHireZone.SetActive(true)

[컷씬3: OnPrisonFull]
  UpgradeManager.HandlePrisonFull()
    │
    ▼
  _cameraController.PlayCutscene(_prisonExpandCutsceneTarget.position, OnPrisonFullCutsceneDone)
    │
    ▼
  OnPrisonFullCutsceneDone()
    └─ PrisonExpandZone.SetActive(true)

[컷씬4: PrisonExpand 완료 후 확장된 감옥 → GameEndUI]
  PrisonExpandZone.OnUpgradeCompleted()
    │
    ├─ SwapFence() (펜스 교체 즉시)
    │
    └─ _cameraController.PlayCutscene(_expandedAreaTarget.position, OnExpandCutsceneDone)
         │
         ▼
       OnExpandCutsceneDone()
         └─ PrisonManager.Instance.ExpandCapacity()
              │
              ▼
            OnPrisonExpanded 발행
              └─ GameEndUI.HandlePrisonExpanded() → Show()

[생산 진동]
  ProductionRoutine() 시작 → _shakeEffect?.Loop()
  StopProduction()        → _shakeEffect?.Stop()

[감옥 진동]
  PrisonZone.HandlePrisonFull() → _shakeEffect?.Play(duration, magnitude)
```

## EDGE_CASES
```
| 상황 | 처리 | 기획서 근거 |
|------|------|------------|
| _cameraController == null | Logger.Error + 콜백 직접 실행(폴백) | 씬 고정 구조라 발생 안 함 |
| 컷씬 재생 중 두 번째 컷씬 트리거 | StartCutscene에서 _isCutscenePlaying 체크 후 무시 | 01_Overview "두 컷씬은 동시 발생 불가" |
| ShakeEffect 미연결 | null 허용 — ?.Loop() / ?.Play() 패턴으로 무시 | 연출 선택사항 |
```

## MODULES

### [MODULE-A] ShakeEffect 신규 컴포넌트
신규: Assets/Scripts/Util/ShakeEffect.cs
  - [ ] ShakeEffect.cs 생성 — MonoBehaviour                                  [P0]
  - [ ] [SerializeField] float _defaultDuration / _defaultMagnitude           [P0]
  - [ ] Play(float duration, float magnitude) — 1회성 진동 Coroutine          [P0]
  - [ ] Loop() — 무한 루프 진동 (생산 라인용)                                 [P0]
  - [ ] Stop() — Coroutine 중단 + localPosition 원위치 복원                   [P0]
  - [ ] _shakeCoroutine 참조 보관 (명시적 StopCoroutine)                      [P0]
  - [ ] 파일 헤더 요약 주석 작성                                              [P0]

### [MODULE-B] CameraController PlayCutscene API + 구독 제거
수정: Assets/Scripts/Managers/CameraController.cs
  - [ ] Start() / OnDisable() 이벤트 구독 블록 제거                           [P0]
  - [ ] HandleFirstSaleCompleted() / HandlePrisonFull() 메서드 삭제           [P0]
  - [ ] CutsceneRoutine → onComplete: Action 파라미터 추가                    [P0]
  - [ ] onComplete?.Invoke() 호출 (복귀 완료 후, 입력 해제 전)                [P0]
  - [ ] public void PlayCutscene(Vector3 targetWorldPos, Action onComplete) 추가 [P0]
  - [ ] _drillUpgradeZoneTransform / _prisonExpandZoneTransform 필드 제거     [P0]
  - [ ] 수정 로그 헤더 추가                                                   [P0]

### [MODULE-C] UpgradeManager 컷씬 콜백 연동
수정: Assets/Scripts/Managers/UpgradeManager.cs
  - [ ] [SerializeField] CameraController _cameraController 추가              [P0]
  - [ ] [Header] Transform 3개: _drillCutsceneTarget / _miningWorkerCutsceneTarget / _prisonExpandCutsceneTarget [P0]
  - [ ] HandleFirstSaleCompleted() → PlayCutscene 호출로 교체                 [P0]
  - [ ] OnFirstSaleCutsceneDone() 콜백 추가                                   [P0]
  - [ ] HandlePrisonFull() → PlayCutscene 호출로 교체                         [P0]
  - [ ] OnPrisonFullCutsceneDone() 콜백 추가                                  [P0]
  - [ ] NotifyDrillCompleted() → PlayCutscene 호출로 교체                     [P0]
  - [ ] OnDrillCutsceneDone() 콜백 추가                                       [P0]
  - [ ] _cameraController null 폴백: Logger.Error + 콜백 직접 실행            [P0]
  - [ ] 수정 로그 헤더 추가                                                   [P0]

### [MODULE-D] ProductionManager ShakeEffect 연동
수정: Assets/Scripts/Managers/ProductionManager.cs
  - [ ] [SerializeField] ShakeEffect _shakeEffect 추가                        [P1]
  - [ ] ProductionRoutine() — OnProductionStarted 직후 _shakeEffect?.Loop()   [P1]
  - [ ] StopProduction() — OnProductionStopped 직후 _shakeEffect?.Stop()      [P1]
  - [ ] 수정 로그 헤더 추가                                                   [P1]

### [MODULE-E] PrisonZone ShakeEffect 연동
수정: Assets/Scripts/Zones/Prison/PrisonZone.cs
  - [ ] [SerializeField] ShakeEffect _shakeEffect 추가                        [P1]
  - [ ] [SerializeField] float _shakeOnFullDuration / _shakeOnFullMagnitude   [P1]
  - [ ] HandlePrisonFull() → _shakeEffect?.Play(_shakeOnFullDuration, _shakeOnFullMagnitude) [P1]
  - [ ] 수정 로그 헤더 추가                                                   [P1]

### [MODULE-F] PrisonExpandZone 컷씬 연동 (확장 완료 → GameEndUI 전 컷씬)
수정: Assets/Scripts/Zones/Upgrade/PrisonExpandZone.cs
  - [ ] [SerializeField] CameraController _cameraController 추가              [P0]
  - [ ] [SerializeField] Transform _expandedAreaTarget (컷씬 목표 위치)       [P0]
  - [ ] OnUpgradeCompleted() 수정 — SwapFence() 즉시 → PlayCutscene 호출     [P0]
  - [ ] PrisonManager.ExpandCapacity() 호출을 콜백으로 이동 (OnExpandCutsceneDone) [P0]
  - [ ] _cameraController null 폴백: Logger.Error + 직접 ExpandCapacity() 호출 [P0]
  - [ ] 수정 로그 헤더 추가                                                   [P0]

## REVIEW_NOTES
```
기획서 일관성: ✗ — 08_UI.md CameraController 구독 이벤트 명세(직접 구독)가 이번 설계와 상충
              → update-design으로 갱신 필요
컷씬2(DrillCompleted): 기획서 미명시, 요청에서 확정 반영
ShakeEffect 수치: 기획서 미정의 → [SerializeField] 노출로 처리
```

---
## REVIEW_STATUS
```
| 단계          | 상태 | 날짜       | 주요 지적 |
|---------------|------|------------|-----------|
| Cross-Review  | -    | -          | -         |
| Senior-Review | -    | -          | -         |
| Learn-Report  | -    | -          | -         |

verdict:    PENDING
unresolved: []
```
