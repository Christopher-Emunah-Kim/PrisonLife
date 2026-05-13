# CHANGESET — RoastStaffGAS
> 에이전트용 코드 변화 추적.
> 세션 시작 시: PENDING_COMMIT 항목만 확인 (COMMITTED는 건너뜀).
> 최신 항목이 위에.

## READ_STRATEGY
```
세션 시작: status=PENDING_COMMIT 항목만 읽기
compact 트리거: COMMITTED 항목 5개 초과 시
compact 방법: COMMITTED 항목 → 별도 확인 없이 제거 (Plans/completed/에 이미 반영)
```

---

## PrisonLife — MODULE-4~6 + MODULE-13 + ExternalAssets (2026-05-14)
```yaml
- date: 2026-05-14
  plan: PLAN_PrisonLife_v1.0
  commit: "4ec0e3b(chore-settings) 96f5ec2(M4-base) 63d126c(M4-player) 9a896d2(M5) 8fc4726(M6-core) c5945ee(M6-editor) 7f15f53(M13) 2ebb54a(EA-1) 4cb8380(EA-2) c0b81a1(EA-3) a9fb203(EA-4) 15b42d6(anim) 757c1cf(scene)"
  files:
    created:
      - Assets/Scripts/Characters/BaseCharacter.cs       # MODULE-4
      - Assets/Scripts/Characters/PlayerCharacter.cs     # MODULE-4
      - Assets/Scripts/Characters/InventoryComponent.cs  # MODULE-4
      - Assets/Scripts/Input/JoystickController.cs       # MODULE-4
      - Assets/Scripts/Zones/IInteractableZone.cs        # MODULE-5
      - Assets/Scripts/Zones/BaseZone.cs                 # MODULE-5
      - Assets/Scripts/Zones/InteractionZone.cs          # MODULE-5
      - Assets/Scripts/Zones/UpgradeZone.cs              # MODULE-5
      - Assets/Scripts/Zones/GridCell.cs                 # MODULE-6
      - Assets/Scripts/Zones/MiningGrid.cs               # MODULE-6
      - Assets/Scripts/Zones/MiningZone.cs               # MODULE-6
      - Assets/Scripts/Zones/MiningColliderBridge.cs     # MODULE-6
      - Assets/Editor/MiningGridEditor.cs                # MODULE-6 에디터 툴
      - Assets/Prefabs/GridCell.prefab                   # MODULE-6
      - Assets/Scripts/Managers/CameraController.cs      # MODULE-13
      - Assets/Animator/PlayerAnimator.controller        # Animator
      - Assets/Scenes/PlayScene.unity                    # PlayScene
      - Assets/ExternalAssets/** (747개)                 # ExternalAssets
    modified:
      - ProjectSettings/DynamicsManager.asset
      - ProjectSettings/ProjectSettings.asset
      - ProjectSettings/QualitySettings.asset
      - ProjectSettings/TagManager.asset
  summary: "MODULE-4(캐릭터/조이스틱) + MODULE-5(Zone 기반) + MODULE-6(채굴시스템) + MODULE-13(카메라) 구현 + ExternalAssets 추가"
  status: COMMITTED
  bugs_found:
    - "채굴 콜라이더 위치 오류: MiningZone 하위에 배치 → PlayerCharacter 하위 MiningColliderBridge 구조로 재설계"
    - "PlayerCharacter OnTriggerEnter 중복 호출: BaseZone과 이중 처리 → PlayerCharacter에서 제거"
  bugs_fixed:
    - "MiningColliderBridge 패턴으로 채굴 콜라이더를 플레이어에 종속 (이동방향 감지 정확성 확보)"
    - "_halfFullFired 플래그 ConsumeResource에서 리셋 제거 (1회성 이벤트 보장)"
```

---

## PrisonLife — MODULE-1~3 + 규칙 수정 (2026-05-13)
```yaml
- date: 2026-05-13
  plan: PLAN_PrisonLife_v1.0
  commit: null
  files:
    created:
      - Assets/Scripts/Data/GameBalanceData.cs        # MODULE-1: MiningLevelData 구조체 List 방식
      - Assets/Scripts/Data/PrisonerData.cs           # MODULE-1
      - Assets/Scripts/Pool/ObjectPool.cs             # MODULE-2: IPoolable 인터페이스 + 제네릭 풀
      - Assets/Scripts/Managers/GameManager.cs        # MODULE-3: OnGameEnded 이벤트, UpgradeZone 활성화 체인
      - Assets/Scripts/Managers/MoneyManager.cs       # MODULE-3
      - Assets/Scripts/Managers/ProductionManager.cs  # MODULE-3: 생산 Coroutine, OnGoodsBufferChanged<int>
      - Assets/Scripts/Managers/SalesManager.cs       # MODULE-3: OnFirstSaleCompleted
      - Assets/Scripts/Managers/PrisonManager.cs      # MODULE-3: OnPrisonFull / OnPrisonExpanded
    modified:
      - _Design/References/Plans/09_Data.md           # v1.1: MiningLevelData 구조 반영, OnMoneyZoneAccumulated 제거
      - _Design/Plans/active/PLAN_PrisonLife_v1.0.md  # 신규 생성
      - _Design/TODO.md                               # MODULE-1~3 등록
      - .claude/rules/general-code.md                 # Logger 규칙, 파일 헤더/수정 로그 규칙 추가
  summary: "MODULE-1(SO) + MODULE-2(ObjectPool) + MODULE-3(Manager 5종) 구현 + 코딩 규칙 업데이트"
  status: COMMITTED  # 36b604a (MODULE-1) / 5fcf899 (MODULE-2) / 02407cf (MODULE-3)
  bugs_found: []
  bugs_fixed:
    - "ProductionManager TryResumeProduction: goodsBuffer full 상태에서 코루틴 즉시 종료되는 로직 버그 수정"
    - "SalesManager OnMoneyZoneAccumulated 불필요 이벤트 제거 (MoneyManager.OnMoneyChanged로 단일화)"
```

---
<!-- 이전 항목들은 compact됨 (2026-04-14) -->

## FORMAT
```yaml
- date: YYYY-MM-DD
  plan: PLAN_[시스템명]_vX.X
  commit: null | "abc1234"
  files:
    modified: []
    created:  []
    deleted:  []
  summary: "한 줄 요약"
  status: PENDING_COMMIT | COMMITTED | REVERTED
  bugs_found: []
  bugs_fixed: []
```
