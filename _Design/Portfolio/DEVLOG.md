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