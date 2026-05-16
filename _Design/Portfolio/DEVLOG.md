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