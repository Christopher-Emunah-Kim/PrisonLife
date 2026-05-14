# TODO — RoastStaffGAS
> 에이전트+사용자 공용. 세션 시작 시 CLAUDE.md 다음으로 읽는다.
> 최신 작업이 위에.
> ★ 설계 기준: _Design/References/Systems/게임 시스템 개선안 v2.0.md (2026-04-17 확정)

## STATUS_KEY
```
[ ] OPEN | [>] ACTIVE | [x] DONE(커밋해시) | [~] DEFERRED | [!] BLOCKED
[P0] 이번세션 | [P1] 다음세션 | [P2] 백로그
```

---

## ACTIVE_WORK
<!-- 진행 중. 완료 FEATURE는 COMPLETED_LOG로 압축 이동 -->

## [FEATURE] PrisonLife 전체 구현 | PLAN_PrisonLife_v1.0
> 시작: 2026-05-12 | 기획서: 01~09_*.md | Phase 1 진행 중

### [MODULE-1] ScriptableObject 데이터 레이어
신규: GameBalanceData.cs / PrisonerData.cs
  - [x] GameBalanceData SO 구현 (09_Data.md 스키마 그대로, 확정 수치 기본값)     [P0]
  - [x] PrisonerData SO 구현                                                       [P0]

### [MODULE-2] ObjectPool<T>
신규: ObjectPool.cs
  - [x] ObjectPool<T> 제네릭 구현 (초기 사이즈 SerializeField, 소진 시 동적 생성) [P0]

### [MODULE-3] Manager 레이어 5종
신규: GameManager.cs / MoneyManager.cs / ProductionManager.cs / SalesManager.cs / PrisonManager.cs
  - [x] GameManager: 씬 초기화, TriggerGameEnd, OnFirstSaleCompleted 릴레이        [P0]
  - [x] MoneyManager: Add/Spend API, OnMoneyChanged 발행                            [P0]
  - [x] ProductionManager: 생산 Coroutine, 4종 static event (OnGoodsBufferChanged<int>) [P0]
  - [x] SalesManager: OnSalesCompleted / OnFirstSaleCompleted 발행                  [P0]
  - [x] PrisonManager: OnPrisonFull / OnPrisonExpanded / OnPrisonCountChanged       [P0]

### [MODULE-4] ✓ COMMITTED 96f5ec2, 63d126c 2026-05-14
신규: BaseCharacter.cs / PlayerCharacter.cs / InventoryComponent.cs / JoystickController.cs
  - [x] BaseCharacter: _moveSpeed/_rotationSpeed, _inputBlocked, 이동/회전          [P0]
  - [x] JoystickController: New Input System, 동적 조이스틱, 데드존                 [P0]
  - [x] PlayerCharacter: Zone API 호출 연결                                          [P0]
  - [x] InventoryComponent: int[] 슬롯, MAX 체크, OnResourceHalfFull 임계치 이벤트  [P0]

### [MODULE-5] ✓ COMMITTED 9a896d2 2026-05-14
신규: IInteractableZone.cs / BaseZone.cs / InteractionZone.cs / UpgradeZone.cs
  - [x] IInteractableZone 인터페이스                                                 [P0]
  - [x] BaseZone: LayerMask 필터, _tickCoroutine 보관, abstract OnPlayerEnter/Exit  [P0]
  - [x] InteractionZone: _tickInterval, DropTick/PickupTick, MAX 체크               [P0]
  - [x] UpgradeZone: _totalCost/_remainingCost/_isCompleted, abstract OnUpgradeCompleted [P0]

### [MODULE-6] ✓ COMMITTED 8fc4726, c5945ee 2026-05-14
신규: GridCell.cs / MiningGrid.cs / MiningZone.cs / MiningColliderBridge.cs / MiningGridEditor.cs / GridCell.prefab
  - [x] GridCell: _isMinable/_isReserved, DOTween 스케일(0.2s)                      [P0]
  - [x] MiningGrid: 8×16 배열, GetNearestMinableCell, 리젠 Coroutine(3f)            [P0]
  - [x] MiningZone: 단계별 콜라이더 전환 + _miningMeshes                            [P0]

### [MODULE-7] ✓ COMMITTED 7edfba0,659fddf,afb7668,c657adf,945562f,d9e46a0 2026-05-15
신규: ResourceDropZone.cs / GoodsPickupZone.cs / ResourceFlyObject.cs / StackMeshItem.cs / MaxIndicatorUI.cs(stub)
  - [x] ResourceDropZone: 자원 투입 틱, DOTween 포물선 연출, 버퍼 메시 적층, MAX UI
  - [x] GoodsPickupZone: 완제품 픽업 틱, OnGoodsPickupCompleted 최초 1회 발행, 버퍼 메시 적층, MAX UI
  - [x] ObjectPool<T> 순수 C# 클래스로 전환 (생성자 방식)
  - [x] InventoryComponent 백팩/쟁반 더미 메시 적층 (소켓 3개 + ObjectPool<StackMeshItem>)
  - [x] ProductionManager ResourceBufferMax / GoodsBufferMax 프로퍼티 추가
  - [x] PlayerCharacter FlySocket 추가
  - [ ] [에디터] ResourceFlyObject Prefab / StackMeshItem Prefab 3종 생성 후 슬롯 연결  [P0]
  - [~] [검토] 버퍼/백팩 메시 단위(5개) ON/OFF → 1개 단위 점진적 쌓임 연출로 개선 필요한지 폴리싱 단계에서 판단 | [P2]

### [MODULE-8] 판매 Zone 3종
신규: SalesDeskZone.cs / SalesZone.cs / MoneyZone.cs
  - [ ] SalesDeskZone: 쟁반 전량 내려놓기, MAX UI                                   [P0]
  - [ ] SalesZone: 3조건 체크, 판매 틱, OnFirstSaleCompleted 최초 1회               [P0]
  - [ ] MoneyZone: 누적 → 전액 이전, DOTween 흡수 연출(0.3f)                        [P0]

### [MODULE-9] Prisoner + PrisonerSpawner + PrisonZone
신규: Prisoner.cs / PrisonerSpawner.cs / PrisonZone.cs
  - [ ] Prisoner: Initialize() 리셋, 말풍선 UI 참조, 직선 이동                      [P0]
  - [ ] PrisonerSpawner: ObjectPool<Prisoner>, 스폰 Coroutine(3f), 큐 MAX(5)        [P0]
  - [ ] PrisonZone: 수용 카운터, 메시 40개 순서대로 ON, OnPrisonFull 발행           [P0]

### [MODULE-10] UpgradeZone 5종
신규: DrillUpgradeZone.cs / TractorUpgradeZone.cs / TractorColliderController.cs / MiningWorkerHireZone.cs / SalesWorkerHireZone.cs / PrisonExpandZone.cs
  - [ ] DrillUpgradeZone: 2단계 전환, TractorUpgradeZone.SetActive                  [P0]
  - [ ] TractorUpgradeZone: 3단계 전환                                               [P0]
  - [ ] TractorColliderController: 별도 컴포넌트, Enable/Disable 제어               [P0]
  - [ ] MiningWorkerHireZone: MiningWorker 3명 스폰                                  [P0]
  - [ ] SalesWorkerHireZone: SalesWorker 1명 스폰                                    [P0]
  - [ ] PrisonExpandZone: ExpandCapacity(40), GameManager.TriggerGameEnd()           [P0]

### [MODULE-13] ✓ COMMITTED 7f15f53 2026-05-14
신규: CameraController.cs
  - [x] LateUpdate 즉시 추적                                                          [P0]
  - [x] PlayCutscene Coroutine (lerp→대기→복귀), 입력 블록                           [P0]
  - [x] OnFirstSaleCompleted / OnPrisonFull 구독                                     [P0]

### [MODULE-14] HUD + UI
신규: HUDController.cs / MaxIndicatorUI.cs / GameEndUI.cs
  - [ ] HUDController: Safe Area, OnMoneyChanged 구독, PLAY 버튼, 음소거             [P0]
  - [ ] MaxIndicatorUI: World Space Canvas, SetVisible API                            [P0]
  - [ ] GameEndUI: OnPrisonExpanded 구독, DOTween 페이드인, Time.timeScale=0         [P0]

### [MODULE-11] MiningWorker FSM                                                      [P1]
신규: MiningWorker.cs
  - [ ] FSM: Moving/Mining, 셀 예약/해제, 채굴 틱, DOTween 날아가는 연출            [P1]

### [MODULE-12] SalesWorker 루프                                                      [P1]
신규: SalesWorker.cs
  - [ ] 상태 머신: WaitingAtTrigger/MovingToBuffer/MovingToDesk/MovingToTrigger      [P1]
  - [ ] OnGoodsBufferChanged(int) 구독 대기/재개                                     [P1]

### [MODULE-15] 인디케이터 화살표 + TutorialSystem                                   [P1]
신규: IndicatorArrow.cs / TutorialSystem.cs
  - [ ] IndicatorArrow: 보빙 Coroutine, 시야 판별                                    [P1]
  - [ ] TutorialSystem: 10단계 상태 머신, DD1/DD2 이벤트 구독                       [P1]

### [MODULE-16] SFXManager + 말풍선 UI                                                [P1]
신규: SFXManager.cs / PrisonerBubbleUI.cs / PrisonCounterUI.cs
  - [ ] SFXManager: null-safe PlayOneShot                                             [P1]
  - [ ] PrisonerBubbleUI: 수량 TMP, No Cell 표시                                     [P1]
  - [ ] PrisonCounterUI: OnPrisonCountChanged 구독, 색상 전환                        [P1]

### [MODULE-17] 조작 유도 UI + Physics Layer                                          [P2]
신규: TutorialHandUI.cs
  - [ ] TutorialHandUI: 첫 터치 시 영구 비활성                                       [P2]
  - [ ] Physics Layer Matrix 설정 (MCP manage_physics)                               [P2]


---

## NEXT_SESSION
(없음)

---

## BACKLOG
<!-- 의존성 기반 우선순위 정렬. 플랜 없음 → 착수 전 /planning 필수 -->
<!-- ★ 설계 기준: _Design/References/Systems/게임 시스템 개선안 v2.0.md (2026-04-17 확정) -->

### [P3] 인게임 HUD 교체 + 입력 리바인딩
<!-- 도화가 스킬 리팩터링 이후 착수 -->

- [ ] Q/E 바인딩 삭제 + 숫자키 1~6 추가 (Enhanced Input)                         [P3]
- [ ] SkillManagerSubsystem SKILL_SLOT_COUNT 2→6                                  [P3]
- [ ] 인게임 HUD 상단 바 (HP/EXP/BossHP)                                          [P3]
- [ ] 인게임 HUD 하단 바 (무기 슬롯 3 + 스킬 슬롯 6)                              [P3]

### [P3] 소서리스 스킬 6개 (두 번째 캐릭터)
<!-- 도화가 스킬 리팩터링 + HUD(P3) 완료 후 착수 -->

- [ ] 소서리스 스킬 스펙 확정 (Temp 파일 기반 보완)                               [P3]
- [ ] 속성 GE (화염 DoT / 냉기 이속감소 / 번개 감전) 구현                         [P3]
- [ ] 소서리스 스킬 6개 구현                                                       [P3]

### [P2] 기능 확장 (독립 작업 가능)

- [ ] 설정 UI 및 SaveData 연동 (EUIID::SETTING + RDS::SetSettingsData)            [P2]
- [ ] 게임 배속 관리 기능 (CustomTimeDilation or WorldSettings)                   [P2]
- [ ] 에너미 BP별 KnockdownMontage 할당 (BP_MeleeEnemy, BP_RangedEnemy, BP_EliteEnemy, BP_BossEnemy) [P2]
- [ ] 스킬 GA BP별 CastingMontage 할당 (BP_GA_Skill1~6)                          [P2]
      AnimMontage 세팅 방법:
        1. AnimMontage 열기 → Notifies 트랙 우클릭 → Add Notify → AnimNotify_SendGameplayEvent 선택
        2. 노티파이 클릭 → Details에서 Tag = "Event.Montage.HitCheck" 입력
        3. 노티파이를 히트 판정 원하는 프레임으로 드래그
        4. GA BP의 CastingMontage 슬롯에 해당 몽타주 할당

---

## DEFERRED
<!-- "나중에" 항목. 이유+우선순위 필수 -->
[~] PullVortex 파라미터 DT 컬럼화 검토 — EditDefaultsOnly 현행 유지. 스킬 파라미터 통합 후 재검토 | [P3]
[~] 도화가 1번 흩뿌리기 — 몽타주 연출 보강 후 재점검 | [P2]
[~] HawkGauge 시스템 + 호크샷 — 게이지 전용 Attribute + UI 추가 비용. 3번째 캐릭터(호크아이) 때 처리 | [P3]
[x] ChargeAndRelease 타입 (스나이프) — MODULE-7~8 완료 (4efe08474, 0eac6728c)
[x] 호크아이 스킬 전체 — MODULE-1~9 완료 (PLAN_Hawkeye_Skills_v1.0)
[x] BP 투사체 NiagaraComp 회전 버그 — 다른 방향으로 완료 처리 (2026-05-05)
[~] 캐릭터/무기 해금 연결 — 해금 시스템 삭제로 불필요해짐. 방향 전환 시 재검토 | [HOLD]

---

## COMPLETED_LOG
[x] Painter06 HomingBounce Lifetime 연장 — 유도 전 소멸 수정 | 0cbfc7167 | 2026-05-06 | ad-hoc
[x] LevelUpSubsystem 재초기화 버그픽스 (EXP 구독 누락) | 6aaad8f19 | 2026-05-06 | PLAN_LevelUpSubsystemReinit_v1.0
[x] PullVortexActor FX 스폰 Z 오프셋 보정 | 467f7f0c3 | 2026-05-06 | PLAN_PullVortexFXZOffset_v1.0
[x] WeaponSelectWidget 일시정지 버그 수정 (bPausesGame + IsAnyPausingUIOpen) | b32c3208e,64962955c | 2026-05-06 | PLAN_PauseBugFix_v1.0
[x] 적 피격 애니메이션 HitMontage + 공격 몽타주 + ABP 에셋 | 6c2406d1a,1f8fa07f9,bd76571ee | 2026-05-06 | PLAN_EnemyHitMontage_v1.0
<!-- compact 형식: [x] FEATURE명 | 커밋 | 날짜 | 플랜파일 -->
[x] 적 피격 애니메이션 HitMontage + GA 좀비 버그 픽스 | 6c2406d1a,1f8fa07f9 | 2026-05-06 | PLAN_EnemyHitMontage_v1.0
[x] 호크아이 스킬 6종 구현 (MODULE-1~9) | 4efe08474,0eac6728c,ca97eb737 | 2026-05-04 | PLAN_Hawkeye_Skills_v1.0
[x] 캐릭터 메시 인게임 연동 | 3624c6080 | 2026-05-03 | PLAN_CharacterMeshApply_v1.0
[x] 데미지 인디케이터 HUD 비네트 | 621229423,b05088290,402fa1e55 | 2026-04-29 | PLAN_DamageIndicator_v1.0
[x] SR + 학습 리포트 (CombatInfra+SkillSystemArch+SkillActivationRefactor 합산) + SR_Fix | b69e4b867,ceca6206e | 2026-04-26 | PLAN_SR_Fix_v1.0
[x] SkillActivationType 3축 분리 리팩터링 + DT_CharacterSkill 통폐합 + Pierce BUG_FIX | 73b04c3b7,d7d0e50b5,691f6e1ef,eea16f8d6,93b86a62e,83b270b00 | 2026-04-26 | PLAN_SkillActivationRefactor_v1.0
[x] BossHPBarWidget HUD 자식 편입 리팩터링 | 3ba19c363,72f7479b2,e4ec76175,fec8946ac | 2026-04-26 | PLAN_BossHPBarRefactor_v1.0
[x] Game ms 최적화 (AIC/BT/CMC/Anim Tick + LOD + 적 수 조정) | 8b6c5c06f,70c8790b5,888c88484,f55a7967b,0cad93d20 | 2026-04-24 | PLAN_GameMsOpt_v1.0
[x] 퍼포먼스 최적화 (SkillFX·BT 프리로드 + GC 스파이크 수정) | 15a2e7198,0734f2ab6 | 2026-04-24 | PLAN_AsyncLoadOpt_v1.0
[x] 스킬 시스템 아키텍처 개선 (ISkillEffectInterface + DT data-driven + PullVortexActor + ElementColor) | 11407aa16,9654d150e,52a2610a4,3e9b82688,ee0b6c1b2,df93b7515 | 2026-04-21~22 | PLAN_SkillSystemArch_v1.0
[x] 도화가 스킬 3·5·6번 구현 (환영의문/먹물세례/콩콩이) | b17d85a38,3bfb35458,1f5cf3bc1 | 2026-04-21~22 | ad-hoc
[x] 아웃게임 3D 로비 + 스테이지 선택 개편 | c6fd4228c,9c6e1a738 | 2026-04-21 | PLAN_OutgameLobby3D_v1.0
[x] P0 전투 인프라 (피격반응/ProjectileSpawn/GroundEffect/CC/몽타주) | bba4030c9,b60ae6522,303a59e87,385652b6e,f6b45a9bf | 2026-04-17~21 | PLAN_CombatInfra_v1.0
[x] EnemySpawner NavMesh 스폰 위치 버그 수정 + RSGameMode 스트리밍 레벨 대기 | 75ba1d80b,6ecf988c7 | 2026-04-16 | PLAN_EnemySpawnFix_v1.0
[x] 캐릭터 스탯 팝업 HUD (MODULE 1~3) | 58de4f52b,d2be49f02,5d1ba8d47 | 2026-04-14 | PLAN_CharacterStatPopup_v1.0
[x] PHASE-1 인게임 루프 완성 (MODULE 1~7 + 에디터) | cd024b49~8b1e18e42 | 2026-04-13~14 | PLAN_Phase1_InGame_v1.0
[x] 패시브 슬롯 UI (PassiveSlotWidget + SlotContainer + PC 연결) | 0a9533b9b,7c36f7fd3,ab9d95674,c2a58ec1a | 2026-04-15 | PLAN_PassiveSlotUI_v1.0
[x] TransitionGameMode FinishLoading 타이밍 수정 (MODULE 1~3) | c5588b2 | 2026-04-09 | PLAN_TransitionFinishLoading_v1.0
[x] Enemy Ranged + Elite + Boss 시스템 (MODULE 1~7) | 에디터 에셋 포함 | 2026-04-06~09 | PLAN_EnemyExpansion_v1.0
[x] Boss HP Bar UI 파이프라인 (C++ MODULE 1~4) | d43254d,b7e3b05,02d92c1,c45823d | 2026-04-09 | PLAN_BossHPBar_v1.0
[x] LastPlayedStage UX 연속성 복원 | 7d586e6,34e2df6,7279505 | 2026-04-09 | PLAN_LastPlayedStageRestore_v1.0
[x] PlayerStatusBarWidget HP/EXP TextBlock 실시간 갱신 | d22fcd1 | 2026-04-09 | ad-hoc
[x] 풀링 시스템 중앙화 + AsyncPreWarm + GC리팩토링 | af3c5cd,8011f7f | 2026-04-08 | PLAN_PoolingCentralize_v1.0
[x] 캐릭터 스킬 슬롯 UI 통합 (UX 빈슬롯 숨김 + CharacterSkillSlot Q/E) | a8e0d582a,ef8d96bcd,6905263d8 | 2026-04-14 | PLAN_SkillSlotUI_v1.0
[x] 스테이지 클리어 로직 + 결과 UI (WBP 포함) | 78378ee,6c7f997,067b08a,4bb9bcb | 2026-04-05~06 | PLAN_StageResult_v1.0
[x] 게임플로우 데이터(RuntimeDS/캐릭터적용) | d622281,f10dab1,783b4e7 | 2026-04-02~03 | PLAN_GameFlow_Data_v1.0
[x] OutGame 선택 UI(캐릭터·스테이지 선택) | 1a21f8c~8d68391,2944f16,71f1ac0,1ed214f,15ac6d4 | 2026-04-01~02 | PLAN_OutGame_SelectUI_v1.0
[x] 게임플로우 레벨(Intro/Transition/OutGame) | c0be61f,2118448,6c2c881,553dc01 | 2026-04-01 | PLAN_GameFlow_Levels_v1.0
[x] 게임플로우 인프라(UIManager/GameInstance) | 2e8a443,3feef02,389e200,f05f3c9,df16494,8150d93 | 2026-03-31 | PLAN_GameFlow_Infra_v1.0
[x] 무기강화+교체UI(CurveTable) | b6b18d4,ed1513a,e35d380,0587332,07153a4,aa6d657 | 2026-03-30 | PLAN_WeaponUpgrade_Replace_v1.0
[x] EXP전달-레벨업UI | 82444aa,e7a5cb6 | 2026-03-28 | PLAN_EXP_LevelUp_UI_v1.0
[x] 플레이어 HP바 위젯 | 714cff4 | 2026-03-28 | PLAN_PlayerHPBarWidget_v1.0
[x] 수동 발사 클릭 미인식 버그 픽스 | 3e21a68 | 2026-03-27 | PLAN_ManualFireBugFix_v1.0
