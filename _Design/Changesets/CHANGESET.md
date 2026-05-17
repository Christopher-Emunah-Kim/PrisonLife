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

## PrisonLife — CutsceneCallback + ShakeEffect (2026-05-17)
```yaml
- date: 2026-05-17
  plan: PLAN_CutsceneCallback_ShakeEffect_v1.0
  commit: ""
  files:
    created:
      - Assets/Scripts/Util/ShakeEffect.cs
    modified:
      - Assets/Scripts/Managers/CameraController.cs
      - Assets/Scripts/Managers/UpgradeManager.cs
      - Assets/Scripts/Managers/ProductionManager.cs
      - Assets/Scripts/Zones/Prison/PrisonZone.cs
      - Assets/Scripts/Zones/Upgrade/PrisonExpandZone.cs
  summary: "컷씬 완료 후 Zone 활성화 콜백 구조 + ShakeEffect 공용 컴포넌트 (생산 라인/감옥 진동) + PrisonExpand → GameEndUI 순서 보장"
  status: PENDING_COMMIT
  bugs_found: []
  bugs_fixed: []
```

---

## PrisonLife — MODULE-17 (2026-05-17)
```yaml
- date: 2026-05-17
  plan: PLAN_PrisonLife_v1.0
  commit: "38cac58"
  files:
    created:
      - Assets/Scripts/UI/TutorialHandUI.cs  # 첫 터치 시 영구 비활성
    modified:
      - ProjectSettings/DynamicsManager.asset  # Physics Layer Matrix 설정
  summary: "MODULE-17: TutorialHandUI + Physics Layer Matrix (Player/Worker/Prisoner/Zone/MiningGrid/MiningCollider)"
  status: COMMITTED
  bugs_found: []
  bugs_fixed: []
```

---

## PrisonLife — HUD 소지금 갱신 버그 수정 (2026-05-17)
```yaml
- date: 2026-05-17
  plan: PLAN_PrisonLife_v1.0
  commit: "515f2f9"
  files:
    modified:
      - Assets/Scripts/Characters/Player/InventoryComponent.cs  # ConsumeMoney → Subtract 연동
      - Assets/Scripts/Managers/MoneyManager.cs                 # Subtract 메서드 추가
  summary: "ConsumeMoney 후 MoneyManager.Subtract 호출 — HUD OnMoneyChanged 갱신 누락 수정"
  status: COMMITTED
  bugs_found:
    - "InventoryComponent.ConsumeMoney가 내부 슬롯만 차감하고 MoneyManager에 알리지 않아 HUD 미갱신"
  bugs_fixed:
    - "MoneyManager.Subtract 추가 (잔액 조건 없이 OnMoneyChanged 발행) + ConsumeMoney에서 호출"
```

---

## PrisonLife — 코드 품질 리팩토링 (2026-05-17)
```yaml
- date: 2026-05-17
  plan: PLAN_PrisonLife_v1.0
  commit: "c06af36(fix-InteractionZone) c404969(refactor-Managers) 13a64de(refactor-UI)"
  files:
    created:
      - Assets/Scripts/Util/Singleton.cs              # 제네릭 싱글턴 베이스 클래스
      - Assets/Scripts/UI/BaseProgressUI.cs           # 공통 프로그레스바 베이스 클래스
    deleted:
      - Assets/Scripts/UI/MaxIndicatorUI.cs           # SetActive 래퍼 수준 — 불필요 제거
    modified:
      - Assets/Scripts/Zones/Base/InteractionZone.cs  # WaitForSeconds 캐싱, protected virtual Awake
      - Assets/Scripts/Zones/Base/UpgradeZone.cs      # WaitForSeconds 캐싱
      - Assets/Scripts/Zones/Interaction/GoodsPickupZone.cs  # base.Awake() 호출, TryResumeProduction 제거
      - Assets/Scripts/Zones/Interaction/MoneyZone.cs         # base.Awake() 호출
      - Assets/Scripts/Zones/Interaction/ResourceDropZone.cs  # base.Awake() 호출
      - Assets/Scripts/Zones/Interaction/SalesDeskZone.cs     # base.Awake() 호출
      - Assets/Scripts/Zones/Mining/MiningGrid.cs             # WaitForSeconds 캐싱
      - Assets/Scripts/Zones/Sales/SalesZone.cs               # WaitForSeconds 캐싱
      - Assets/Scripts/Characters/Prisoner/PrisonerSpawner.cs # WaitForSeconds 캐싱
      - Assets/Scripts/Managers/GameManager.cs                # Singleton<T> 상속, instance event
      - Assets/Scripts/Managers/MoneyManager.cs               # Singleton<T> 상속, instance event
      - Assets/Scripts/Managers/ProductionManager.cs          # Singleton<T> 상속, instance event, ConsumeGoods 내 TryResumeProduction
      - Assets/Scripts/Managers/SalesManager.cs               # Singleton<T> 상속, instance event
      - Assets/Scripts/Managers/PrisonManager.cs              # Singleton<T> 상속, instance event
      - Assets/Scripts/Managers/SFXManager.cs                 # Singleton<T> 상속
      - Assets/Scripts/Managers/TutorialSystem.cs             # Singleton<T> 상속
      - Assets/Scripts/Managers/UpgradeManager.cs             # instance event 구독 방식
      - Assets/Scripts/Managers/CameraController.cs           # instance event 구독 방식
      - Assets/Scripts/UI/HUDController.cs                    # instance event 구독 방식
      - Assets/Scripts/UI/GameEndUI.cs                        # instance event 구독 방식
      - Assets/Scripts/UI/PrisonCounterUI.cs                  # instance event 구독 방식
      - Assets/Scripts/UI/PrisonerBubbleUI.cs                 # BaseProgressUI 상속
      - Assets/Scripts/UI/UpgradeProgressUI.cs                # BaseProgressUI 상속
      - Assets/Scripts/Util/Logger.cs                         # Conditional 어트리뷰트 방식
      - Assets/Scripts/Zones/Prison/PrisonZone.cs             # instance event 구독 방식
      - Assets/Scripts/Zones/Upgrade/MiningWorkerHireZone.cs  # instance event 구독 방식
      - Assets/Scripts/Zones/Upgrade/SalesWorkerHireZone.cs   # instance event 구독 방식
  summary: "WaitForSeconds 캐싱 + Singleton<T> 베이스 + instance event 전환 + BaseProgressUI 추출 + Logger Conditional"
  status: COMMITTED
  bugs_found:
    - "InteractionZone Awake() private 선언 → 하위 4개 클래스 private Awake hiding으로 base.Awake 미호출 → _tickWait null → 매 프레임 틱"
  bugs_fixed:
    - "protected virtual Awake() + 하위 protected override + base.Awake() 호출 패턴으로 수정"
```

---

## PrisonLife — MODULE-10 (2026-05-17)
```yaml
- date: 2026-05-17
  plan: PLAN_PrisonLife_v1.0
  commit: "b21555f(refactor-Zones) 40efd36(UpgradeManager) c3a2442(DrillUpgradeZone) e063432(UpgradeProgressUI)"
  files:
    created:
      - Assets/Scripts/Zones/Upgrade/DrillUpgradeZone.cs    # MiningLevel(1) + NotifyDrillCompleted()
      - Assets/Scripts/Zones/Upgrade/TractorUpgradeZone.cs  # MiningLevel(2), SetActive(false)
      - Assets/Scripts/Zones/Upgrade/MiningWorkerHireZone.cs # 채굴 인부 3명 스폰
      - Assets/Scripts/Zones/Upgrade/SalesWorkerHireZone.cs  # 판매 인부 1명 스폰
      - Assets/Scripts/Zones/Upgrade/PrisonExpandZone.cs    # ExpandCapacity() + FenceSwap
      - Assets/Scripts/Managers/UpgradeManager.cs           # Zone 활성화 순서 제어 싱글톤
      - Assets/Scripts/UI/UpgradeProgressUI.cs              # World Space 프로그레스바 UI
    moved:
      - Zones/*.cs → Zones/Base/ Mining/ Interaction/ Sales/ Prison/ Upgrade/ (도메인 분리)
    modified:
      - Assets/Scripts/Zones/Base/UpgradeZone.cs            # DOTween 흡수 연출 + UpgradeProgressUI 연동
      - Assets/Scripts/Zones/Mining/MiningZone.cs           # OnPlayerEnter 시 InitMaxValues 적용
      - Assets/Scripts/Managers/GameManager.cs              # 업그레이드 Zone 레퍼런스 제거 (SRP)
      - Assets/Resources/Data/GameBalanceData.asset
      - Assets/Scenes/PlayScene.unity
  summary: "MODULE-10: UpgradeZone 5종 + UpgradeManager + 폴더 구조 개편 + UpgradeProgressUI"
  status: COMMITTED
  bugs_found:
    - "PowerShell 파일 이동 시 Unity GUID 재발급 → Script Missing"
    - "DrillUpgradeZone SetActive 후 자식 비활성 상태로 미표시"
    - "MiningZone.UpgradeMiningLevel 시점에 플레이어 Zone 이탈 → InitMaxValues 미적용"
  bugs_fixed:
    - "git show HEAD:path.meta로 원본 GUID 복원 (15개 파일)"
    - "사용자가 Inspector에서 자식 오브젝트 직접 활성화"
    - "InitMaxValues를 OnPlayerEnter 진입 시점으로 이전"
```

---

## PrisonLife — MODULE-9 (2026-05-17)
```yaml
- date: 2026-05-17
  plan: PLAN_PrisonLife_v1.0
  commit: "0ac1369(Prisoner) 07105ea(PrisonerSpawner) 7dae784(PrisonZone) 2a93a67(SalesZone) 42d3fc0(chore)"
  files:
    created:
      - Assets/Scripts/Characters/Prisoner.cs        # 직선 이동, 구매 수량 기반 말풍선
      - Assets/Scripts/Characters/PrisonerSpawner.cs # ObjectPool, FIFO 큐, No Cell late-join
      - Assets/Scripts/Zones/PrisonZone.cs           # 그리드 동적 생성, Prisoner 수용
      - Assets/Scripts/UI/PrisonerBubbleUI.cs        # TMP + Filled 프로그레스바 + No Cell
      - Assets/Prefabs/Prisoner.prefab
    modified:
      - Assets/Scripts/Zones/SalesZone.cs            # 죄수 큐 조건 연결, 틱 단위 처리
      - Assets/Scripts/Data/PrisonerData.cs          # prisonColumns/Spacing 추가
      - Assets/Scripts/Zones/MiningGrid.cs           # var → 타입 명시
      - Assets/Resources/Data/PrisonerData.asset
      - Assets/Resources/Data/GameBalanceData.asset
      - Assets/Scenes/PlayScene.unity
  summary: "MODULE-9(Prisoner+PrisonerSpawner+PrisonZone+PrisonerBubbleUI) + SalesZone 죄수 큐 연결"
  status: COMMITTED
  bugs_found:
    - "PrisonZone OnTriggerEnter: 자식 Collider 감지 실패 (TryGetComponent → GetComponentInParent)"
    - "MoveTowardPrison direction*100f 방식: SalesZone 기준 방향으로 인한 이동 오차"
    - "OnPrisonFull 이후 신규 dequeue 죄수 No Cell 미표시"
  bugs_fixed:
    - "GetComponentInParent<Prisoner>() 로 변경"
    - "방향 벡터 대신 목적지 위치 직접 전달 (_prisonTarget Transform)"
    - "DequeueFront 시점 IsFull 즉시 체크로 late-join No Cell 처리"
```

---

## PrisonLife — MODULE-8 (2026-05-16)
```yaml
- date: 2026-05-16
  plan: PLAN_PrisonLife_v1.0
  commit: "72abb15(SalesDeskZone) 963eda2(SalesZone) 89d806c(MoneyZone) 2150be7(fix-coroutine+data) cdd4de7(chore) 7c01be0(docs)"
  files:
    created:
      - Assets/Scripts/Zones/SalesDeskZone.cs   # MODULE-8: 쟁반→책상 1개씩 틱 이전
      - Assets/Scripts/Zones/SalesZone.cs        # MODULE-8: 3조건 판매 틱
      - Assets/Scripts/Zones/MoneyZone.cs        # MODULE-8: 판매금 누적→백팩 이전
    modified:
      - Assets/Scripts/Zones/InteractionZone.cs  # fix: yield break 시 _tickCoroutine null 처리
      - Assets/Scripts/Zones/UpgradeZone.cs      # fix: yield break 시 _tickCoroutine null 처리
      - Assets/Scripts/Data/GameBalanceData.cs   # fix: backpackMoneyMax 기본값 1200 추가
      - Assets/Scenes/PlayScene.unity
      - _Design/TODO.md
      - _Design/PromptLog/PROMPT_LOG.md
      - _Design/Portfolio/DEVLOG.md
  summary: "MODULE-8(SalesDeskZone+SalesZone+MoneyZone) + Coroutine yield break null 처리 + backpackMoneyMax 기본값 수정"
  status: COMMITTED
  bugs_found:
    - "yield break 자가 종료 시 _tickCoroutine stale reference 잔류"
    - "GameBalanceData.backpackMoneyMax 기본값 0 → IsMoneyFull() 항상 true"
  bugs_fixed:
    - "yield break 직전 _tickCoroutine = null 추가 (InteractionZone, UpgradeZone, SalesZone)"
    - "GameBalanceData.backpackMoneyMax = 1200 기본값 추가"
```

---

## PrisonLife — MODULE-7 (2026-05-15)
```yaml
- date: 2026-05-15
  plan: PLAN_PrisonLife_v1.0
  commit: "7edfba0(ObjectPool-refactor) 659fddf(InventoryComponent) afb7668(ResourceDropZone) c657adf(GoodsPickupZone) 945562f(prefabs) d9e46a0(TMP) a011e4a(docs)"
  files:
    created:
      - Assets/Scripts/Zones/ResourceDropZone.cs        # MODULE-7: 자원 투입 Zone
      - Assets/Scripts/Zones/GoodsPickupZone.cs         # MODULE-7: 완제품 픽업 Zone
      - Assets/Scripts/Zones/ResourceFlyObject.cs       # MODULE-7: DOTween 포물선 연출
      - Assets/Scripts/Characters/StackMeshItem.cs      # MODULE-7: IPoolable 더미 메시
      - Assets/Scripts/UI/MaxIndicatorUI.cs             # MODULE-7: MAX 인디케이터 stub
      - Assets/Prefabs/FlyObject_Resource.prefab        # 연출 프리팹
      - Assets/Prefabs/FlyObject_Goods.prefab           # 연출 프리팹
      - Assets/Prefabs/StackMesh_Resource.prefab        # 더미 메시 프리팹
      - Assets/Prefabs/StackMesh_Goods.prefab           # 더미 메시 프리팹
      - Assets/Prefabs/StackMesh_Money.prefab           # 더미 메시 프리팹
    modified:
      - Assets/Scripts/Pool/ObjectPool.cs               # MonoBehaviour → 순수 C# 클래스 전환
      - Assets/Scripts/Characters/InventoryComponent.cs # 소켓 3개 + ObjectPool<StackMeshItem> 적층 연출
      - Assets/Scripts/Characters/PlayerCharacter.cs    # FlySocket 추가
      - Assets/Scripts/Managers/ProductionManager.cs    # ResourceBufferMax / GoodsBufferMax 프로퍼티
      - _Design/TODO.md
      - Assets/Scenes/PlayScene.unity
      - Assets/Scenes/TestScene.unity
  summary: "MODULE-7(ResourceDropZone+GoodsPickupZone+ResourceFlyObject+StackMeshItem) + ObjectPool 순수 C# 전환 + InventoryComponent 소켓 기반 메시 적층"
  status: COMMITTED
  bugs_found:
    - "FlyObject가 world origin(0,0,0)으로 날아가는 문제: Fly() 호출 전 transform.position 미설정"
    - "플레이어 피벗이 발 위치여서 FlySocket 위치 부정확"
    - "GoodsPickupZone OnTick에서 MAX 체크가 픽업 자체를 막는 문제"
    - "돈 메시가 자원 도착 시 소켓 1→2로 이동하지 않는 문제 (ReparentIfNeeded 누락)"
  bugs_fixed:
    - "Fly() 첫 줄에 transform.position = from 추가"
    - "PlayerCharacter에 FlySocket([SerializeField] Transform) 추가"
    - "MAX UI 로직을 HandleGoodsBufferChanged 이벤트 핸들러로 완전 이관"
    - "ReparentIfNeeded() 메서드로 소켓 전환 시 부모 변경 처리"
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
