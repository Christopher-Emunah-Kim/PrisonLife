# [Prison Life] 06_Zone_Prison 기획서 v1.0

---

# 💡 핵심 역할

감옥 Zone은 판매 완료된 죄수를 수용하는 구역이다. 초기 한도 20명, 확장 업그레이드 후 40명까지 수용한다. 죄수 풀링과 감옥 내 메시 가시성 관리로 구현한다.

---

# 💡 다른 시스템과 관계

| 연관 시스템 | 관계 방향 | 설명 |
|---|---|---|
| SalesZone | 판매 완료 → 이동 | 판매 완료 죄수가 감옥 Zone으로 이동 |
| PrisonerSpawner | 카운트 관리 | 미결재 죄수 카운트 기반으로 스폰 |
| UpgradeZone | 업그레이드 | 감옥 확장 업그레이드 Zone |
| GameBalanceData | 데이터 조회 | 감옥 초기 한도, 확장 한도 참조 |

---

# 💡 기능 명세 및 상세 규칙

## 감옥 Zone 구조

- 초기 수용 한도: 20명 (`GameBalanceData._prisonInitialCapacity`)
- 확장 후 수용 한도: 40명 (`GameBalanceData._prisonExpandedCapacity`)
- 감옥 내 죄수 메시: 전체 한도(40명)만큼 미리 스폰, `SetActive(false)` 대기
- 수용 카운터: 감옥 입구에 `현재수용/한도` 형태로 TMP 표시

## 죄수 수용 흐름

1. 판매 완료 죄수가 감옥 방향(판매 서있는 방향)으로 계속 직선 이동
2. 감옥 Zone 입장 트리거에 충돌 시:
   - 버퍼에 여유 있으면: 감옥 내 대기 메시 중 다음 순서 `SetActive(true)`, 길 위 죄수 풀링 반환
   - 버퍼 가득 참(현재수용 == 한도): 길 위에서 계속 이동하지 않고 대기, 머리 위 "No Cell!" 말풍선 표시
3. 수용 카운터 갱신 (`현재수용/한도` TMP)
4. 현재수용 == 한도 시: 카운터 색상 흰색 → 빨간색 전환

## 카운터 색상 규칙

| 상태 | 색상 |
|---|---|
| 여유 있음 (현재수용 < 한도) | 흰색 |
| 가득 참 (현재수용 == 한도) | 빨간색 |
| 확장 후 (현재수용 < 새 한도) | 흰색 복귀 |

## 만원 시 처리

1. 현재수용 == 한도 (20명) 도달
2. 길 위 죄수 "No Cell!" 말풍선 표시
3. 감옥이 터지려는 연출 재생
4. 컷씬: 카메라가 감옥 확장 업그레이드 Zone으로 이동
5. 감옥 확장 업그레이드 Zone `SetActive(true)`

## 감옥 확장 업그레이드

- 확장 비용: `GameBalanceData._prisonExpandCost`
- 완료 시:
  1. 옆에 새 구역 추가 (메시 전환 또는 새 Zone SetActive)
  2. 수용 한도 20 → 40으로 갱신
  3. 추가분 20명 메시 스폰 후 `SetActive(false)` 대기
  4. "No Cell!" 말풍선 소거
  5. 카운터 색상 흰색 복귀
  6. 게임 종료 처리 시작

## 게임 종료 처리

- 감옥 확장 업그레이드 완료 시 트리거
- 게임 일시 정지 (`Time.timeScale = 0`)
- 화면 중앙에 게임 로고 표시
- 로고 탭 시 [STORE_URL] 이동

---

# 💡 죄수 스포너 (PrisonerSpawner)

## 스폰 조건

- 스폰 위치: 9시 방향 끝
- 미결재 죄수 카운트를 항상 유지
- 판매 처리 완료 시 카운트 -1 → 스포너에 스폰 요청
- 죄수 큐가 최대 인원(5명)에 도달하면 스폰 중단, 여유 생길 때까지 대기

## 죄수 구매 수량 설정

- 스폰 시 구매 수량 랜덤 결정: `PrisonerData._purchaseMin` ~ `PrisonerData._purchaseMax` (2~4)
- 구매 수량에 따른 말풍선 수량 표시
- 구매 수량 × 완제품 단가 = 해당 죄수의 총 지불금액

## 풀링 구조

- 풀링 대상: 길 위를 이동하는 죄수 오브젝트
- 초기 풀 사이즈: `[SerializeField] int _poolSize = 30`
- 감옥 내 메시는 별도 (풀링 대상 아님, 미리 스폰 후 가시성 관리)

---

# 💡 데이터 설계

## 주요 수치

| 항목 | 위치 | 기본값 |
|---|---|---|
| 감옥 초기 한도 | GameBalanceData._prisonInitialCapacity | 20 |
| 감옥 확장 한도 | GameBalanceData._prisonExpandedCapacity | 40 |
| 감옥 확장 비용 | GameBalanceData._prisonExpandCost | [TBD] |
| 죄수 스폰 간격 | PrisonerData._spawnInterval | [TBD] |
| 죄수 큐 최대 인원 | PrisonerData._maxQueueSize | 5 |
| 구매 수량 범위 | PrisonerData._purchaseMin/_purchaseMax | 2~4 |

## static event 목록

```csharp
public static event Action OnPrisonFull;      // 감옥 만원 시
public static event Action OnPrisonExpanded;  // 감옥 확장 완료 시
```
