# PLAN_PrisonLife_v1.0
```yaml
date:    2026-05-12
sprint:  SPRINT-1
status:  ACTIVE
designs: [01_Overview.md, 02_Player.md, 03_Zone_Mining.md, 04_Zone_Production.md,
          05_Zone_Sales.md, 06_Zone_Prison.md, 07_Upgrade.md, 08_UI.md, 09_Data.md]
```

## GOAL
> 자원 채굴 → 생산 → 판매 → 죄수 수용의 단일 씬 루프를 ScriptableObject 데이터 드리븐 + Manager 싱글톤 + BaseZone/BaseCharacter 상속 계층으로 구현하는 플레이어블 광고 게임.

## SCOPE
```yaml
new_files:
  # MODULE-1
  - Assets/Scripts/Data/GameBalanceData.cs
  - Assets/Scripts/Data/PrisonerData.cs
  # MODULE-2
  - Assets/Scripts/Pool/ObjectPool.cs
  # MODULE-3
  - Assets/Scripts/Managers/GameManager.cs
  - Assets/Scripts/Managers/MoneyManager.cs
  - Assets/Scripts/Managers/ProductionManager.cs
  - Assets/Scripts/Managers/SalesManager.cs
  - Assets/Scripts/Managers/PrisonManager.cs
  # MODULE-4
  - Assets/Scripts/Characters/BaseCharacter.cs
  - Assets/Scripts/Characters/PlayerController.cs
  - Assets/Scripts/Characters/InventoryComponent.cs
  - Assets/Scripts/Input/JoystickController.cs
  # MODULE-5
  - Assets/Scripts/Zones/IInteractableZone.cs
  - Assets/Scripts/Zones/BaseZone.cs
  - Assets/Scripts/Zones/InteractionZone.cs
  - Assets/Scripts/Zones/UpgradeZone.cs
  # MODULE-6
  - Assets/Scripts/Zones/GridCell.cs
  - Assets/Scripts/Zones/MiningGrid.cs
  - Assets/Scripts/Zones/MiningZone.cs
  # MODULE-7
  - Assets/Scripts/Zones/ResourceDropZone.cs
  - Assets/Scripts/Zones/GoodsPickupZone.cs
  # MODULE-8
  - Assets/Scripts/Zones/SalesDeskZone.cs
  - Assets/Scripts/Zones/SalesZone.cs
  - Assets/Scripts/Zones/MoneyZone.cs
  # MODULE-9
  - Assets/Scripts/Characters/Prisoner.cs
  - Assets/Scripts/Managers/PrisonerSpawner.cs
  - Assets/Scripts/Zones/PrisonZone.cs
  # MODULE-10
  - Assets/Scripts/Zones/DrillUpgradeZone.cs
  - Assets/Scripts/Zones/TractorUpgradeZone.cs
  - Assets/Scripts/Zones/TractorColliderController.cs
  - Assets/Scripts/Zones/MiningWorkerHireZone.cs
  - Assets/Scripts/Zones/SalesWorkerHireZone.cs
  - Assets/Scripts/Zones/PrisonExpandZone.cs
  # MODULE-11
  - Assets/Scripts/Characters/MiningWorker.cs
  # MODULE-12
  - Assets/Scripts/Characters/SalesWorker.cs
  # MODULE-13
  - Assets/Scripts/Managers/CameraController.cs
  # MODULE-14
  - Assets/Scripts/UI/HUDController.cs
  - Assets/Scripts/UI/MaxIndicatorUI.cs
  - Assets/Scripts/UI/GameEndUI.cs
  # MODULE-15
  - Assets/Scripts/UI/IndicatorArrow.cs
  - Assets/Scripts/UI/TutorialSystem.cs
  # MODULE-16
  - Assets/Scripts/Managers/SFXManager.cs
  - Assets/Scripts/UI/PrisonerBubbleUI.cs
  - Assets/Scripts/UI/PrisonCounterUI.cs
  # MODULE-17
  - Assets/Scripts/UI/TutorialHandUI.cs
modified_files: []
new_datatables: []
new_tags:       []
```

## CONFIRMED_VALUES
> 2026-05-12 시니 확정 수치 — GameBalanceData / PrisonerData SerializeField 기본값으로 사용

```yaml
# 채굴
miningTickInterval_1:   0.5f
miningTickInterval_2:   0.33f
miningTickInterval_3:   0.17f
backpackResourceMax_1:  20
backpackResourceMax_2:  30
backpackResourceMax_3:  60
miningRegenTime:        3f

# 생산/판매
resourceBufferMax:      30
goodsBufferMax:         20
productionTime:         2f
salesDeskMax:           30
goodsPrice:             10

# 업그레이드 비용
drillUpgradeCost:       50
tractorUpgradeCost:     150
miningWorkerCost:       100
salesWorkerCost:        80
prisonExpandCost:       200

# 죄수
spawnInterval:          3f

# 캐릭터
moveSpeed:              5f
rotationSpeed:          10f

# DOTween
flyDuration:            0.4f     # 날아가는 연출 (자원/돈)
scaleDownDuration:      0.2f     # 스케일 축소 (채굴)
moneyAbsorbDuration:    0.3f     # 돈 빨려들어가는 연출
```

## DESIGN_DECISIONS
> 기획서 이슈 확정 결정 (2026-05-12)

```yaml
DD1_OnResourceHalfFull:
  결정: InventoryComponent에서 slot[0] >= MAX의 50% 시 OnResourceHalfFull 발행
  위치: InventoryComponent.AddResource() 내부 임계치 체크

DD2_OnGoodsPickupCompleted:
  결정: GoodsPickupZone에서 traySlot 최초 1개 이상 적재 완료 시 1회만 발행
  위치: GoodsPickupZone 틱 Coroutine 내 _hasCompletedOnce 플래그

DD3_UpgradeZoneActivation:
  결정: OnFirstSaleCompleted 발행 시 DrillUpgradeZone + MiningWorkerHireZone + SalesWorkerHireZone 동시 SetActive(true)
  위치: GameManager가 3개 Zone 참조 보유, OnFirstSaleCompleted 구독

DD4_TractorCollider:
  결정: TractorColliderController 별도 컴포넌트로 분리 (MiningZone에서 Enable/Disable 제어)
  위치: Assets/Scripts/Zones/TractorColliderController.cs

DD5_OnGoodsBufferChanged:
  결정: ProductionManager.OnGoodsBufferChanged에 int count 파라미터 추가
  시그니처: static event Action<int> OnGoodsBufferChanged
```

## INTEGRATION_POINTS
```yaml
owner:       GameManager
entry:       GameManager.Awake → SO 로드 → Manager 초기화 → PrisonerSpawner.StartSpawning → TutorialSystem.Init
depends_on:  없음 (신규 프로젝트)
ref_pattern: 없음 (신규 프로젝트)
arch_impact: |
  CLASS_REGISTRY 전면 신규 등록 (44개 클래스)
  INTEGRATION_MAP 전체 구성
  PATTERNS 확립
```

## FLOW
```
[GameManager.Awake]
    │
    ▼
[SO 주입: GameBalanceData, PrisonerData]
    │
    ▼
[Manager 초기화: Money/Production/Sales/Prison]
    │
    ▼
[PrisonerSpawner.StartSpawning]   [TutorialSystem.Init]
    │
    ▼
[플레이어 조이스틱 입력 대기]
    │
    ├─→ [MiningZone 진입] → 채굴 틱 → InventoryComponent.AddResource()
    │       └─→ [ResourceDropZone] → ProductionManager.AddResource()
    │               └─→ [생산 Coroutine] → ProductionManager.AddGoods()
    │                       └─→ [GoodsPickupZone] → InventoryComponent.AddGoods()
    │                               └─→ [SalesDeskZone] → SalesZone 재고 증가
    │                                       └─→ [SalesZone 틱] → MoneyZone 누적
    │                                               └─→ [MoneyZone 진입] → MoneyManager.Add()
    │
    ├─→ [OnFirstSaleCompleted] → DrillUpgradeZone/MiningWorkerHireZone/SalesWorkerHireZone SetActive
    │
    ├─→ [OnPrisonFull] → CameraController.PlayCutscene(PrisonZone) → PrisonExpandZone SetActive
    │
    └─→ [OnPrisonExpanded] → GameManager.TriggerGameEnd() → Time.timeScale=0 → GameEndUI 표시
```

## PHASE_PLAN
```
Phase 1 (P0 코어) — MODULE 1~10, 13~14
  목표: 플레이어가 채굴→생산→판매→감옥 루프를 수동 플레이 가능
  모듈 순서: 1→2→3→5→4→6→7→8→9→10→13→14

Phase 2 (P1 자동화+UI) — MODULE 11~12, 15~16
  목표: 인부 자동화 + 튜토리얼 인디케이터 동작
  모듈 순서: 11→12→15→16

Phase 3 (P2 폴리싱) — MODULE 17 + DOTween 연출 + SR
  목표: 빌드 검증 완료, 배포 가능 상태
```

## EDGE_CASES
```
| 상황 | 처리 | 기획서 근거 |
|------|------|------------|
| 백팩 MAX 상태로 MiningZone 진입 | 채굴 틱 스킵, MaxIndicatorUI 표시 | 02_Player.md |
| 자원 버퍼 MAX 상태로 ResourceDropZone 진입 | 투입 틱 스킵, MaxIndicatorUI 표시 | 04_Zone_Production.md |
| 완제품 버퍼 MAX 시 생산 Coroutine 정지 | OnGoodsBufferChanged(count) 구독자가 버퍼 full 확인 | 04_Zone_Production.md |
| 판매 책상 MAX 상태 | SalesDeskZone 투입 틱 스킵, MaxIndicatorUI 표시 | 05_Zone_Sales.md |
| 죄수 큐 5명 MAX | PrisonerSpawner 추가 스폰 중단 | 05_Zone_Sales.md |
| 감옥 만원 상태에서 죄수 추가 수용 시도 | No Cell 말풍선, 죄수 대기 유지 | 06_Zone_Prison.md |
| UpgradeZone 완료 후 재진입 | _isCompleted=true → 틱 즉시 종료 | 07_Upgrade.md |
| 컷씬 중 플레이어 이동 | _inputBlocked=true → 조이스틱 입력 무시 | 01_Overview.md |
| SalesWorker 완제품 버퍼 빔 | OnGoodsBufferChanged(count) 구독 대기 후 count>0 시 이동 재개 | 04_Zone_Production.md |
```

## REVIEW_NOTES
```
기획서 일관성: ✓ (이슈 5개 DD1~DD5로 확정 처리)
누락 예외처리: 없음
기획서 정정:   DD1~DD5 결정 사항을 09_Data.md에 반영 권장
Gemini 반영:   미실시
```

---
## REVIEW_STATUS
```
| 단계          | 상태    | 날짜       | 주요 지적 |
|---------------|---------|------------|-----------|
| Cross-Review  | -       | -          | -         |
| Senior-Review | -       | -          | -         |
| Learn-Report  | -       | -          | -         |

verdict:    PENDING
unresolved: []
```
