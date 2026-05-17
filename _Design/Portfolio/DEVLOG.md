# DEVLOG — PrisonLine 기술 의사결정 로그
> 포트폴리오·기술문서용. 파이프라인(PLAN/CODE/SR)에서 자동 기록.
> 독자: 면접관, 팀장, 채용 담당자 — 기술 판단력을 보여주는 서사 중심.

## 기록 기준 (아래 중 하나 해당 시만 기록)
- 선택지 2개 이상을 실제로 검토한 설계 결정
- 버그 원인이 즉각 자명하지 않았던 경우 (진단 과정 포함)
- 성능·메모리·구조 트레이드오프가 명확히 존재
- UE/GAS/C++의 비자명한 패턴 활용 (UPROPERTY 생명주기, ASC 소유권 등)

## 타입
| 타입 | 의미 |
|------|------|
| `ARCH` | 구조/설계 결정 — 클래스 책임 분리, 시스템 경계 |

---


---

<!-- 새 항목은 가장 최근 날짜가 위로 오도록 추가 -->

## [2026-05-17] [BUG_FIX] instance event 전환 후 OnEnable 구독 타이밍 버그

**상황**
`static event → instance event` 리팩토링 직후 플레이 테스트에서 버퍼 메시가 화면에 전혀 표시되지 않는 현상 발생. 자원 투입·생산·픽업 로직은 정상 동작(인벤토리에 물건이 들어옴)하고, 숫자 카운트도 쌓이지만 `ResourceDropZone`과 `GoodsPickupZone`의 3D 메시 적층만 렌더링되지 않았다. 풀에 있는 프리팹들이 전부 비활성 상태인 것을 확인.

**문제·과제**
`ObjectPool.Get()`은 `SetActive(true)`를 호출하므로 풀 자체는 정상. 메시 갱신 진입점인 `RefreshBufferMeshes()`가 호출되지 않는 것이 원인이었다. 이 메서드는 `HandleXxxBufferChanged` 이벤트 핸들러 안에 있는데, 핸들러 자체가 등록되지 않은 상태였다.

구독 코드:
```csharp
private void OnEnable()
{
    ProductionManager.Instance.OnResourceBufferChanged += HandleResourceBufferChanged;
}
```

`OnEnable`은 해당 오브젝트의 `Awake` 직후에 호출된다. 그런데 Unity 씬 로드 순서에서 `ResourceDropZone`의 `OnEnable`이 `ProductionManager`의 `Awake`(= `Singleton<T>.Awake`, Instance 할당 시점)보다 먼저 실행되면 `Instance == null` → `NullReferenceException` → **구독 자체가 실패**. 이후 버퍼가 변경돼도 핸들러가 호출되지 않아 메시 갱신이 전혀 일어나지 않았다.

이전 `static event` 방식은 `ClassName.OnEvent += handler` 형태로 Instance 없이 타입만으로 구독 가능했기 때문에 이 문제가 없었다. instance event 전환이 이 타이밍 의존성을 수면 위로 드러냈다.

**검토한 선택지**
- `OnEnable` 유지 + null 가드: `if (Instance != null)` 조건 추가 — null이면 구독 누락, 근본 해결 안 됨
- `Start()`로 구독 이전: 씬의 모든 오브젝트 `Awake`가 완료된 후 `Start`가 실행되므로 Instance 보장 가능
- Singleton에 지연 구독 큐 추가: 과도한 복잡도 — 이 규모에서 불필요

**결정**
구독을 `OnEnable` → `Start()`로 이전. `OnDisable`에는 null 가드 추가. `ResourceDropZone`·`GoodsPickupZone`은 `Start()`에서 현재 버퍼 값으로 핸들러를 즉시 1회 호출해 초기 메시 상태를 동기화(씬 시작 시 이미 버퍼에 수량이 있는 경우 대비).

영향 파일 9개 일괄 수정: `ResourceDropZone`, `GoodsPickupZone`, `HUDController`, `CameraController`, `GameEndUI`, `PrisonZone`, `GameManager`, `UpgradeManager`, `PrisonCounterUI`.

**결과**
버퍼 메시 적층 정상 렌더링 확인. NullReferenceException 9건 제거.

**포트폴리오 포인트**
Unity 라이프사이클에서 `OnEnable`과 `Start`의 실행 순서 차이가 만드는 구독 타이밍 버그. `static event`는 Instance 없이 구독 가능하지만 instance event는 반드시 Instance가 살아있어야 하므로, 이 전환이 기존에 묵시적으로 숨겨져 있던 라이프사이클 의존성을 표면화시킨 사례. `Start()`는 씬 내 모든 오브젝트의 `Awake`가 완료된 후 실행됨이 보장되므로 싱글턴 참조 구독의 안전한 진입점.

**관련 파일**
`Assets/Scripts/Zones/Interaction/ResourceDropZone.cs`, `GoodsPickupZone.cs`, `Assets/Scripts/UI/HUDController.cs`, `GameEndUI.cs`, `PrisonCounterUI.cs`, `Assets/Scripts/Managers/CameraController.cs`, `GameManager.cs`, `UpgradeManager.cs`, `Assets/Scripts/Zones/Prison/PrisonZone.cs`

---

## [2026-05-17] [PATTERN] Singleton\<T\> 베이스 추출 + static event → instance event 전환

**상황**
매니저 7개(`GameManager`, `MoneyManager`, `SalesManager`, `SFXManager`, `UpgradeManager`, `TutorialSystem`, `PrisonManager`, `ProductionManager`)에 동일한 싱글턴 보일러플레이트가 반복되고 있었다. 또한 `public static event`로 선언된 이벤트들이 매니저 생존 여부와 무관하게 어디서든 구독 가능한 구조였다.

**문제·과제**
- 보일러플레이트 반복: `Instance`, `Awake()` 중복 감지 패턴이 7개 클래스에 복사됨 — 유지보수 단일 변경점 없음
- `static event`의 위험: 매니저가 `Destroy`된 후에도 구독자가 이벤트 리스트에 남아 메모리 릭 가능. 또한 클래스 타입만 알면 Instance 없이도 구독 가능 — 매니저 생존과 이벤트 접근의 결합이 묵시적

**검토한 선택지**
- `static event` 유지: 기존 코드 그대로 — 위험 유지
- `instance event` 전환만: 구독 측에서 `Manager.Instance.OnEvent` 경유 필수화 — Instance null 시 명시적 오류로 문제 가시화
- `Singleton<T>` + `instance event` 동시 전환: 보일러플레이트 제거 + 이벤트 접근 명시화 동시 해결

**결정**
`Singleton<T> : MonoBehaviour`를 `Assets/Scripts/Util/`에 추출. `protected virtual OnAwake()` 진입점을 제공해 하위 클래스가 `Awake` override 없이 추가 초기화를 수행하도록 설계. `static event` 11개를 `instance event`로 전환, 구독 측 9개 파일을 `ClassName.Instance.OnEvent` 방식으로 일괄 수정.

**결과**
매니저당 5줄 보일러플레이트 제거(×7 = 35줄). Instance 경유 구독으로 매니저 생존과 이벤트 접근이 명시적으로 연결됨. `OnAwake()`로 `base.Awake()` 누락 위험 구조적 제거.

**포트폴리오 포인트**
제네릭 MonoBehaviour 싱글턴 패턴에서 `OnAwake()` 훅 메서드를 통한 하위 클래스 초기화 설계. `static event`와 `instance event`의 생명주기 차이와 메모리 릭 위험에 대한 실무적 판단.

**관련 파일**
`Assets/Scripts/Util/Singleton.cs`, `Assets/Scripts/Managers/*.cs`

---

## [2026-05-17] [BUG_FIX] Unity MonoBehaviour private Awake 숨김으로 인한 매 프레임 틱 버그

**상황**
`InteractionZone`에 `WaitForSeconds` 캐싱을 위해 `Awake()`를 신규 추가. 이후 `_tickInterval`을 `1.2f`까지 올려도 자원이 쏟아지는 속도가 전혀 줄지 않아 원인을 파악하기 어려운 상태였다.

**문제·과제**
`InteractionZone.Awake()`를 새로 추가했지만, 하위 4개 클래스(`ResourceDropZone`, `GoodsPickupZone`, `SalesDeskZone`, `MoneyZone`)가 이미 `private void Awake()`를 선언하고 있었다. C#에서 `private` 메서드는 상속 관계에서 **오버라이드가 아닌 숨김(hiding)** 처리된다. Unity는 각 클래스의 `Awake`를 독립적으로 실행하지 않고 가장 구체적인 타입의 `Awake`만 실행하므로, 부모의 `Awake`가 완전히 무시되어 `_tickWait == null` 상태가 됐다. `yield return null`과 동일하게 동작 → 매 프레임 `OnTick` 실행.

**검토한 선택지**
- `Start()`로 이동: `Awake` 순서 의존 없이 초기화 가능하나 `OnEnable` 이전 구독 시점 문제
- `Awake virtual` + 하위 `override`: 표준적인 C# 상속 패턴 — `base.Awake()` 호출 강제 가능

**결정**
`InteractionZone.Awake()`를 `protected virtual`로 변경. 하위 4개 클래스를 `protected override Awake()` + `base.Awake()` 호출로 수정. 주석으로 누락 방지 경고 추가.

**결과**
`_tickInterval` 수치 조절이 실제 틱 속도에 정상 반영됨.

**포트폴리오 포인트**
C# `private` vs `protected virtual` 메서드 상속 차이가 Unity MonoBehaviour 라이프사이클에서 야기하는 실제 버그 사례. 캐싱 최적화 작업이 의도치 않게 기존 동작을 깨뜨리는 패턴 — 변경 범위 파악의 중요성.

**관련 파일**
`Assets/Scripts/Zones/Base/InteractionZone.cs`, `Assets/Scripts/Zones/Interaction/*.cs`

---

## [2026-05-17] [BUG_FIX] 이벤트 발행 이후 합류한 오브젝트의 상태 동기화 (Late-Join State Sync)

**상황**
감옥 만원 시 `OnPrisonFull` 이벤트로 큐 대기 죄수 + 이동 중 죄수 전원에게 "No Cell" 말풍선을 표시하는 구조 구현. 그런데 이벤트 발행 이후 새로 판매 완료된 죄수들은 No Cell이 표시되지 않는 버그 발생.

**문제·과제**
`OnPrisonFull`은 감옥이 꽉 찬 순간 1회 발행된다. 그 시점에 `_waitingQueue` + `_movingToPrison` 리스트를 순회해 상태를 전파하지만, 이후에 판매 완료되어 새로 `DequeueFront()`를 통해 이동을 시작하는 죄수는 이미 이벤트가 지나간 후이므로 상태 전파를 받지 못한다.

**검토한 선택지**
- 매 프레임 `IsFull` 폴링: Prisoner가 `Update()`에서 직접 체크 — 불필요한 매 프레임 체크, 책임 위치 부적절
- `OnPrisonFull` 재발행: 매 수용 시도 실패마다 이벤트 재발행 — 이벤트 남발, 기존 구독자 중복 처리 발생
- `DequeueFront()` 시점 즉시 체크: 이동 시작 직후 `PrisonManager.IsFull`을 확인해 만원이면 즉시 No Cell 활성화

**결정**
`DequeueFront()` 내부에서 `MoveTowardPrison()` 호출 직후 `PrisonManager.Instance.IsFull` 체크를 추가. 만원 상태면 해당 죄수에게 즉시 `ShowNoCellBubble(true)` 호출. 이벤트 구독 방식은 유지하되, "이벤트 이후 합류자"는 합류 시점에 현재 상태를 직접 조회해 동기화.

**결과**
감옥 만원 이후 판매 완료된 모든 죄수에게 No Cell이 정상 표시됨. 이벤트 구독 로직 변경 없이 단일 체크 추가로 해결.

**포트폴리오 포인트**
이벤트 기반 시스템에서 "이벤트 발행 시점 이전 구독자"와 "이후 합류자" 모두를 올바르게 처리하는 패턴. 이벤트로 전파하되, 합류 시점에 현재 상태를 직접 조회(pull)하여 동기화하는 hybrid 방식.

**관련 파일**
`Assets/Scripts/Characters/PrisonerSpawner.cs`, `Assets/Scripts/Characters/Prisoner.cs`

---

## [2026-05-16] [BUG_FIX] yield break 시 Coroutine 참조 잔류 버그 수정

**상황**
`SalesZone` 구현 중 코드 리뷰 과정에서 `yield break` 전 `_tickCoroutine = null` 처리 누락을 발견. `InteractionZone`, `UpgradeZone`에도 동일 패턴이 존재해 일괄 수정.

**문제·과제**
Unity Coroutine이 스스로 `yield break`로 종료할 때 `_tickCoroutine` 필드에 죽은 참조(stale reference)가 잔류한다. 이후 플레이어가 Zone에 재진입하면 `OnPlayerEnter → StopTick()` 이 이미 종료된 코루틴에 `StopCoroutine`을 시도하게 된다.

**검토한 선택지**
- 방치: 실제로는 Unity가 종료된 코루틴에 `StopCoroutine`을 호출해도 crash는 없음. 그러나 `OnWorkerEnter`의 `_tickCoroutine == null` 가드가 오동작할 위험이 있음
- `yield break` 직전 `_tickCoroutine = null` 추가: 참조를 명시적으로 정리해 가드 로직이 정확하게 동작하도록 보장

**결정**
`yield break` 직전 `_tickCoroutine = null` 추가. 단, `StopTick()` 경유 종료(`StopCoroutine` + `= null` 이미 처리)는 예외로 적용 불필요.

**결과**
`InteractionZone`, `UpgradeZone`, `SalesZone` 3개 파일 수정. 재진입 시 코루틴 이중 실행 및 가드 오동작 방지.

**포트폴리오 포인트**
Unity Coroutine 생명주기에서 외부 중단(`StopCoroutine`)과 자가 종료(`yield break`)의 참조 처리 차이를 구분하고, 방어 코드를 패턴으로 구조화한 사례.

**관련 파일**
`Assets/Scripts/Zones/InteractionZone.cs`, `UpgradeZone.cs`, `SalesZone.cs`