# [Prison Life] 09_Data 기획서 v1.1

---

# 💡 ScriptableObject 설계

## 분리 기준

| 구분 | 기준 | 해당 SO |
|---|---|---|
| ScriptableObject | 밸런스 수치. 레벨/단계별로 변하거나 여러 시스템이 공유 | GameBalanceData, PrisonerData |
| [SerializeField] | 연출/조작감 수치. 단일 컴포넌트가 단독 소유 | 각 컴포넌트 인스펙터 |

---

## GameBalanceData (SO 1번)

경제/용량/업그레이드 비용 전체 관리. 레벨별 수치 조정 시 이 파일만 수정.

```csharp
[CreateAssetMenu(fileName = "GameBalanceData", menuName = "PrisonLife/GameBalanceData")]
public class GameBalanceData : ScriptableObject
{
    [Header("백팩 용량")]
    public int backpackMoneyMax;        // 백팩 돈 MAX

    [Header("채굴 업그레이드 단계별 수치")]
    // index 0 = 1단계(곡괭이), 1 = 2단계(드릴), 2 = 3단계(트랙터)
    // 인스펙터에서 List 요소를 추가하면 단계 확장 가능
    public List<MiningLevelData> miningLevels;
    // MiningLevelData { tickInterval, backpackMax }
    // 기본값: [(0.5f, 20), (0.33f, 30), (0.17f, 60)]
    // 헬퍼: GetMiningLevel(int levelIndex) — 범위 초과 시 마지막 단계 반환

    [Header("채굴 그리드")]
    public float miningRegenTime;       // 석탄 리젠 시간 [TBD]

    [Header("생산")]
    public int   resourceBufferMax;     // 자원 버퍼 MAX [TBD]
    public int   goodsBufferMax;        // 완제품 버퍼 MAX [TBD]
    public float productionTime;        // 완제품 1개 생산 시간 [TBD]

    [Header("판매")]
    public int   salesDeskMax;          // 판매 책상 완제품 MAX [TBD]
    public int   goodsPrice;            // 완제품 1개 단가 [TBD]

    [Header("업그레이드 비용")]
    public int   drillUpgradeCost;      // 드릴 업그레이드 비용 [TBD]
    public int   tractorUpgradeCost;    // 트랙터 업그레이드 비용 [TBD]
    public int   miningWorkerCost;      // 채굴 인부 고용 비용 [TBD]
    public int   salesWorkerCost;       // 판매 인부 고용 비용 [TBD]
    public int   prisonExpandCost;      // 감옥 확장 비용 [TBD]

    [Header("감옥 수용")]
    public int   prisonInitialCapacity = 20;   // 초기 수용 한도
    public int   prisonExpandedCapacity = 40;  // 확장 후 수용 한도
}
```

---

## PrisonerData (SO 2번)

죄수 행동 수치 독립 관리. GameBalanceData와 분리하여 죄수 행동 튜닝 시 독립적으로 조정.

```csharp
[CreateAssetMenu(fileName = "PrisonerData", menuName = "PrisonLife/PrisonerData")]
public class PrisonerData : ScriptableObject
{
    [Header("스폰")]
    public float spawnInterval;     // 죄수 스폰 간격 [TBD]

    [Header("큐")]
    public int   maxQueueSize = 5;  // 판매 Zone 앞 대기 큐 최대 인원

    [Header("구매")]
    public int   purchaseMin = 2;   // 구매 수량 최솟값
    public int   purchaseMax = 4;   // 구매 수량 최댓값
}
```

---

# 💡 [SerializeField] 에디터 노출 항목 전체 목록

| 컴포넌트 | 멤버 | 기본값 | 비고 |
|---|---|---|---|
| CameraController | _offset | (0, 10, -7) | 카메라 오프셋 |
| CameraController | _cutsceneLerpSpeed | [TBD] | 컷씬 이동 속도 |
| CameraController | _returnDuration | 0.8f | 컷씬 복귀 시간 |
| BaseCharacter | _moveSpeed | [TBD] | 이동 속도 |
| BaseCharacter | _rotationSpeed | [TBD] | lerp 회전 속도 |
| JoystickController | _deadzone | 0.1f | 입력 데드존 |
| InteractionZone | _tickInterval | 1개/틱 | Drop/Pickup 틱 속도 |
| UpgradeZone | _tickInterval | [TBD] | 차감 틱 속도 |
| SalesZone | _salesTickInterval | 2f | 판매 체크 간격 |
| InventoryComponent | _meshUnit | 5 | 더미 메시 표시 단위 |
| IndicatorArrow | _bobAmplitude | [TBD] | 보빙 진폭 |
| IndicatorArrow | _bobSpeed | [TBD] | 보빙 속도 |
| ObjectPool | _poolSize | 30 | 풀 초기 사이즈 |
| TractorController | _shovelColliderSize | [TBD] | 트랙터 삽 콜라이더 크기 |
| DOTween 연출 전체 | duration 각각 | [TBD] | 각 컴포넌트 SerializeField |

---

# 💡 Physics Layer 설계

| 레이어 | 번호 | 충돌 대상 레이어 |
|---|---|---|
| Player | 6 | Zone, MiningGrid |
| Worker | 7 | Zone, MiningGrid |
| Prisoner | 8 | Zone (감옥 입장 트리거만) |
| Zone | 9 | Player, Worker, Prisoner |
| MiningGrid | 10 | Player, Worker |
| Projectile | 11 | MiningGrid (자원 날아가는 연출) |

Layer Collision Matrix에서 위 설정 외 모든 조합 비활성화.

---

# 💡 오브젝트 풀링

| 풀 대상 | 컴포넌트 | 초기 사이즈 |
|---|---|---|
| 죄수 (길 위 이동) | Prisoner | 30 |
| 채굴 자원 (날아가는 연출) | ResourceProjectile | 30 |
| 돈 메시 | MoneyMesh | 30 |
| 완제품 메시 | GoodsMesh | 30 |

- 초기 사이즈: `[SerializeField] int _poolSize = 30` (각 Pool 컴포넌트)
- 풀 소진 시: 새 오브젝트 동적 생성 후 풀에 추가

---

# 💡 static event 전체 목록

```csharp
// MoneyManager
public static event Action<int> OnMoneyChanged;

// ProductionManager
public static event Action OnGoodsBufferChanged;
public static event Action OnResourceBufferChanged;
public static event Action OnProductionStarted;
public static event Action OnProductionStopped;

// SalesManager
public static event Action OnSalesCompleted;
public static event Action OnFirstSaleCompleted;   // 컷씬 트리거
// OnMoneyZoneAccumulated 제거 — MoneyZone은 MoneyManager.Add() 호출 시 OnMoneyChanged로 HUD 갱신 (중복 이벤트 불필요)

// PrisonManager
public static event Action OnPrisonFull;           // 컷씬 트리거
public static event Action OnPrisonExpanded;
public static event Action<int> OnPrisonCountChanged;
```

**공통 규칙**: 모든 이벤트 구독은 `Awake` 또는 `OnEnable`에서, 해제는 반드시 `OnDestroy` 또는 `OnDisable`에서 수행.

---

## 정정 이력

| 버전 | 날짜 | 섹션 | 변경 내용 |
|------|------|------|-----------|
| v1.1 | 2026-05-12 | GameBalanceData | 채굴 단계별 개별 필드(miningTickInterval_1/2/3, backpackResourceMax_1/2/3) → MiningLevelData 구조체 List<MiningLevelData> miningLevels로 대체. GetMiningLevel(int) 헬퍼 추가. backpackResourceMax 단일 필드 제거. |
