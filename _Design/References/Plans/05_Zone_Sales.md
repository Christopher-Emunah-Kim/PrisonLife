# [Prison Life] 05_Zone_Sales 기획서 v1.0

---

# 💡 핵심 역할

판매 Zone은 완제품을 판매 책상에 올려두고, 죄수 큐에서 대기 중인 죄수들에게 판매하여 판매금을 생성하는 구역이다. 플레이어 또는 판매 인부가 판매 트리거 안에 있을 때 판매 틱이 동작한다.

---

# 💡 다른 시스템과 관계

| 연관 시스템 | 관계 방향 | 설명 |
|---|---|---|
| Player | 진입 → 책상 적재 | 쟁반 완제품 → 판매 책상에 내려놓음 |
| Player | 진입 → 판매금 수령 | MoneyZone 진입 시 백팩 돈 적재 |
| SalesWorker | 자율 → 판매 | 완제품 버퍼 → 책상 적재 루프 |
| PrisonerSpawner | 스폰 → 큐 | 죄수 큐에 죄수 공급 |
| PrisonZone | 판매 완료 → 이동 | 판매된 죄수가 감옥 Zone으로 이동 |
| GameBalanceData | 데이터 조회 | 판매 책상 MAX, 완제품 단가 참조 |
| PrisonerData | 데이터 조회 | 죄수 큐 최대 인원, 구매 수량 범위 참조 |

---

# 💡 기능 명세 및 상세 규칙

## Zone 구성

| Zone | 역할 |
|---|---|
| 판매 책상 Zone | 쟁반 완제품 내려놓기 (InteractionZone) |
| 판매 트리거 Zone | 판매 틱 조건 판단 영역 |
| MoneyZone | 판매금 수령 (InteractionZone) |
| 죄수 큐 영역 | 죄수 대기 줄 (책상 방향으로 줄 서기) |

## 판매 책상 Zone

- InteractionZone 상속 (다른 Zone과 동일 패턴)
- 플레이어 진입 시 쟁반 slot[0] 완제품 전량 책상에 내려놓음
- 책상에 이미 완제품이 있고 판매 중이면 추가 적재 가능
- 판매 책상 MAX: `GameBalanceData._salesDeskMax`
- 책상 MAX 시: Zone 위에 MAX UI 표시

## 판매 틱 처리

- 조건: 플레이어 또는 판매 인부가 판매 트리거 Zone 안에 있을 것
- 틱 간격: `[SerializeField] float _salesTickInterval = 2f` (에디터 노출)
- 판매 가능 조건 체크 (매 틱):
  1. 판매 트리거 Zone 안에 플레이어 또는 판매 인부가 있는가
  2. 판매 책상에 완제품이 1개 이상 있는가
  3. 죄수 큐 맨 앞 죄수가 대기 중인가
- 3가지 모두 충족 시 판매 1회 처리

**판매 1회 처리 흐름**
1. 책상 완제품 -1
2. 큐 맨 앞 죄수 말풍선 수량 -1 (구매 진행도 UI 갱신)
3. 판매금 = 완제품 단가 (`GameBalanceData._goodsPrice`)
4. MoneyZone 누적 판매금 +판매금
5. 죄수 말풍선 수량이 0이 되면 해당 죄수 처리 완료 → 감옥 Zone으로 이동

## 죄수 큐

- 큐 최대 인원: `PrisonerData._maxQueueSize = 5`
- 죄수가 큐 맨 앞까지 이동 후 대기
- 판매 처리 완료 시: 처리된 죄수 이동, 뒤 죄수들 앞으로 한 칸씩 이동, 스포너에 1명 스폰 요청
- 책상 완제품 0개 시: 큐 죄수들 그냥 대기 (이탈 없음)

## MoneyZone (판매금 수령)

- InteractionZone 상속
- 판매 틱마다 MoneyZone에 판매금 누적 (HUD 소지금과 분리)
- 플레이어 진입 시: Zone 누적 판매금 전액 → 플레이어 백팩 slot[1]로 DOTween lerp 연출
- HUD 소지금 갱신: `static event Action<int> OnMoneyChanged` 발행
- 판매금 Zone에 1번 튜토리얼 화살표 표시 (최초 판매 완료 후)

---

# 💡 판매 인부 (SalesWorker)

## 고용 조건

- 판매 인부 고용 UpgradeZone 완료 시 1명 스폰
- 스폰 위치: 판매 인부 업그레이드 Zone 위치 (판매 트리거 뒤)
- 활성화 조건: 채굴 인부 고용 Zone과 동시에 SetActive(true)

## 동작 루프

1. 초기 대기: 판매 트리거 Zone 위치에서 대기
2. 판매 책상 완제품 0개 → 완제품 버퍼 Zone으로 이동
3. 완제품 버퍼 Zone 진입: 쟁반 MAX만큼 자동 픽업 (버퍼에 MAX 미만 시 있는 만큼만 픽업)
4. 판매 책상 Zone으로 이동
5. 판매 책상 Zone 진입: 쟁반 전량 책상에 내려놓음
6. 판매 트리거 Zone으로 이동 → 1번으로 복귀

**예외 처리**
- 이동 중 책상이 소진되어도 현재 이동 완료 후 처리
- 완제품 버퍼가 비어있으면 버퍼에 완제품이 생길 때까지 대기

---

# 💡 데이터 설계

## 주요 수치

| 항목 | 위치 | 기본값 |
|---|---|---|
| 판매 책상 MAX | GameBalanceData._salesDeskMax | [TBD] |
| 완제품 단가 | GameBalanceData._goodsPrice | [TBD] |
| 판매 틱 간격 | SerializeField _salesTickInterval | 2초 |
| 죄수 큐 최대 인원 | PrisonerData._maxQueueSize | 5 |
| 죄수 구매 수량 범위 | PrisonerData._purchaseMin/_purchaseMax | 2~4 |

## static event 목록

```csharp
public static event Action OnSalesCompleted;           // 죄수 1명 판매 완료
public static event Action<int> OnMoneyZoneAccumulated; // 판매금 Zone 누적 변경
```
