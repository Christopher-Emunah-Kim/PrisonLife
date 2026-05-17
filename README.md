# Prison Life — Playable Ad Demo

> 자원 채취 → 생산 → 판매 시뮬레이터 장르의 플레이어블 광고 데모.
> Unity 2022.3.62f2 / Android Portrait / 단일 씬 구성.

---

## 게임 개요

플레이어가 채굴 → 생산 → 판매 → 죄수 수용 사이클을 반복하며 감옥을 키워나가는 캐주얼 시뮬레이터.
쿼터뷰 시점에서 동적 조이스틱으로 이동하며, 각 Zone에 진입하는 것만으로 상호작용이 자동 처리된다.

### 핵심 루프

```
채굴(MiningZone)
  → 자원 투입(ResourceDropZone) → 생산(ProductionManager)
    → 완제품 픽업(GoodsPickupZone) → 판매 책상(SalesDeskZone)
      → 판매(SalesZone) → 판매금 수령(MoneyZone)
        → 채굴·인부 업그레이드 / 감옥 확장
          → 죄수 수용(PrisonZone) → 게임 클리어
```

### Zone 배치 (쿼터뷰 기준 시계 방향)

| Zone | 위치 | 역할 |
|---|---|---|
| MiningZone | 1시 | 석탄 채굴 (8×16 그리드) |
| ResourceDropZone | 11시 | 자원 투입 → 생산 트리거 |
| GoodsPickupZone | 9시 | 완제품 픽업 버퍼 |
| SalesDeskZone | 8시 | 쟁반 → 판매 책상 이전 |
| SalesZone | 7시 | 완제품 판매 + 죄수 큐 |
| MoneyZone | 판매 Zone 우측 | 판매금 백팩 이전 |
| PrisonZone | 5시 | 죄수 수용 (초기 한도 20명) |

---

## 기술 스택

| 항목 | 내용 |
|---|---|
| 엔진 | Unity 2022.3.62f2 |
| 언어 | C# |
| 빌드 타겟 | Android (Portrait 고정) |
| 외부 패키지 | DOTween, TextMeshPro, New Input System |
| 씬 구성 | 단일 씬 (PlayScene) |

---

## 아키텍처 개요

### 레이어 구조

```
[Data Layer]       ScriptableObject (GameBalanceData, PrisonerData)
      ↓ Inspector 주입
[Manager Layer]    싱글톤 (GameManager, MoneyManager, ProductionManager,
                            SalesManager, PrisonManager, UpgradeManager)
      ↓ static event Action
[Zone Layer]       BaseZone → InteractionZone / UpgradeZone → 각 Zone
      ↓ OnPlayerEnter / OnPlayerExit
[Character Layer]  BaseCharacter → PlayerCharacter / MiningWorker / SalesWorker
```

### 핵심 설계 원칙

**Zone 틱 방식** — `OnTriggerStay` 미사용. `OnTriggerEnter`에서 Coroutine 시작, `OnTriggerExit`에서 중단.

```csharp
public override void OnPlayerEnter(PlayerCharacter player)
{
    base.OnPlayerEnter(player);
    _tickCoroutine = StartCoroutine(TickRoutine(player));
}
public override void OnPlayerExit(PlayerCharacter player)
{
    base.OnPlayerExit(player);
    // StopTick()은 BaseZone에서 처리
}
```

**시스템 통신** — Manager 싱글톤 + `static event Action`. UI·Zone이 Manager를 직접 호출하지 않고 이벤트만 구독.

```csharp
// 발행 (Manager)
public static event Action OnFirstSaleCompleted;

// 구독 (Zone / UI)
private void OnEnable()  => SalesManager.OnFirstSaleCompleted += Handle;
private void OnDisable() => SalesManager.OnFirstSaleCompleted -= Handle;
```

**UpgradeZone 부분 지불** — 이탈 후에도 잔액 유지. 재진입 시 잔액에서 차감 재개.

**오브젝트 풀링** — `ObjectPool<T>` (순수 C# 생성자 방식). `Initialize()`로 상태 완전 리셋 후 재사용.

---

## 폴더 구조

```
Assets/Scripts/
│
├── Characters/
│   ├── Base/
│   │   └── BaseCharacter.cs          이동·회전 공통 베이스
│   ├── Player/
│   │   ├── PlayerCharacter.cs        Zone API 호출, FlySocket
│   │   ├── InventoryComponent.cs     백팩/쟁반 슬롯, 더미 메시 적층
│   │   └── StackMeshItem.cs          더미 메시 단위 오브젝트
│   ├── Prisoner/
│   │   ├── Prisoner.cs               직선 이동, 말풍선 UI 참조
│   │   └── PrisonerSpawner.cs        ObjectPool 스폰, FIFO 큐
│   └── Worker/                       (MODULE-11/12 MiningWorker·SalesWorker 예정)
│
├── Zones/
│   ├── Base/
│   │   ├── IInteractableZone.cs      Zone 공통 인터페이스
│   │   ├── BaseZone.cs               LayerMask 필터, Coroutine 보관
│   │   ├── InteractionZone.cs        Drop/Pickup 틱 공통 베이스
│   │   └── UpgradeZone.cs            부분 지불, DOTween 흡수 연출, ProgressUI
│   ├── Mining/
│   │   ├── MiningZone.cs             채굴 콜라이더 단계 전환, InitMaxValues
│   │   ├── MiningGrid.cs             8×16 셀 배열, 리젠 Coroutine
│   │   ├── GridCell.cs               채굴 가능 여부, DOTween 스케일
│   │   ├── MiningColliderBridge.cs   플레이어 하위 콜라이더 → MiningZone 중계
│   │   └── ResourceFlyObject.cs      DOTween 포물선 연출 (풀링)
│   ├── Interaction/
│   │   ├── ResourceDropZone.cs       자원 투입 틱, 버퍼 메시, MAX UI
│   │   ├── GoodsPickupZone.cs        완제품 픽업 틱, 버퍼 메시
│   │   ├── SalesDeskZone.cs          쟁반 → 책상 이전 틱
│   │   └── MoneyZone.cs              판매금 전액 이전, 흡수 연출
│   ├── Sales/
│   │   └── SalesZone.cs              3조건 판매 틱, 죄수 큐 처리
│   ├── Prison/
│   │   └── PrisonZone.cs             메시 그리드 동적 생성, 수용 한도
│   └── Upgrade/
│       ├── DrillUpgradeZone.cs       채굴 Level 1 전환
│       ├── TractorUpgradeZone.cs     채굴 Level 2 전환
│       ├── MiningWorkerHireZone.cs   채굴 인부 3명 스폰
│       ├── SalesWorkerHireZone.cs    판매 인부 1명 스폰
│       └── PrisonExpandZone.cs       감옥 확장 + 펜스 교체
│
├── Managers/
│   ├── GameManager.cs                씬 초기화, 게임 종료 흐름
│   ├── MoneyManager.cs               소지금 Add/Spend, OnMoneyChanged
│   ├── ProductionManager.cs          생산 Coroutine, 버퍼 상태 이벤트
│   ├── SalesManager.cs               판매 완료, OnFirstSaleCompleted
│   ├── PrisonManager.cs              수용 카운트, OnPrisonFull/Expanded
│   ├── UpgradeManager.cs             Zone 활성화 순서 제어 (Drill→Tractor 전환)
│   └── CameraController.cs           즉시 추적, 컷씬 Coroutine
│
├── UI/
│   ├── UpgradeProgressUI.cs          World Space 프로그레스바 (UpgradeZone 자식)
│   ├── PrisonerBubbleUI.cs           죄수 말풍선 (수량 TMP + No Cell 표시)
│   └── MaxIndicatorUI.cs             버퍼 MAX 도달 시 월드 UI 표시
│
├── Data/
│   ├── GameBalanceData.cs            밸런스 수치 ScriptableObject
│   └── PrisonerData.cs               죄수 행동 수치 ScriptableObject
│
├── Pool/
│   └── ObjectPool.cs                 제네릭 풀 (순수 C# 생성자, Initialize/Return)
│
├── Input/
│   └── JoystickController.cs         New Input System, 동적 조이스틱, 데드존
│
└── Util/
    └── Logger.cs                     Debug.Log 래퍼 (릴리스 빌드 제거 대응)
```

---

## 주요 클래스 관계

```
GameBalanceData (SO) ──→ 각 Manager / Zone ([SerializeField] 주입)
PrisonerData    (SO) ──→ PrisonerSpawner / SalesZone

GameManager ──── static event ──→ CameraController
SalesManager ─── static event ──→ UpgradeManager, CameraController
PrisonManager ── static event ──→ UpgradeManager, CameraController, GameManager
ProductionManager static event ──→ GoodsPickupZone

PlayerCharacter ─ OnTriggerEnter ──→ BaseZone.OnPlayerEnter()
BaseZone ────────── Coroutine ──────→ InteractionZone / UpgradeZone 틱

MiningZone ────────────────────────→ MiningColliderBridge (PlayerCharacter 하위)
                                     └→ GridCell 충돌 → MiningZone.OnMiningColliderHit()

DrillUpgradeZone.OnUpgradeCompleted()
  → MiningZone.UpgradeMiningLevel(1)
  → UpgradeManager.NotifyDrillCompleted()   // DrillZone off, TractorZone on

PrisonExpandZone.OnUpgradeCompleted()
  → PrisonManager.ExpandCapacity()
  → SwapFence()                              // Original↔Expanded 가시성 교체
  → GameManager.TriggerGameEnd()            // PrisonManager.OnPrisonExpanded 경유
```

---

## 업그레이드 흐름

```
[게임 시작]
  모든 UpgradeZone: SetActive(false)

[첫 판매 완료] SalesManager.OnFirstSaleCompleted
  → DrillUpgradeZone    SetActive(true)
  → MiningWorkerHireZone SetActive(true)
  → SalesWorkerHireZone  SetActive(true)

[DrillUpgrade 완료]
  → DrillUpgradeZone    SetActive(false)
  → TractorUpgradeZone  SetActive(true)
  → MiningZone          Level 1 (드릴 콜라이더 활성)

[TractorUpgrade 완료]
  → TractorUpgradeZone  SetActive(false)
  → MiningZone          Level 2 (트랙터 콜라이더 활성)

[감옥 만원] PrisonManager.OnPrisonFull
  → PrisonExpandZone    SetActive(true)

[PrisonExpand 완료] → 게임 클리어
```

---

## ScriptableObject 스키마

### GameBalanceData
밸런스 수치 전담 SO. 채굴 단계별 틱 간격·MAX치, 생산 시간, 버퍼 상한, 업그레이드 비용 등.

### PrisonerData
죄수 행동 수치 전담 SO. 스폰 간격, 큐 최대 인원, 구매 수량 범위, 감옥 그리드 컬럼·간격.

두 SO는 역할이 다르므로 분리 유지 (GameBalanceData에 통합 금지).
