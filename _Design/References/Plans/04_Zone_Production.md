# [Prison Life] 04_Zone_Production 기획서 v1.0

---

# 💡 핵심 역할

생산 Zone은 자원(석탄)을 투입받아 완제품(수갑)을 생산하고, 완제품 버퍼에 쌓아두는 구역이다. 자원 투입 Drop Zone과 완제품 버퍼 Zone이 물리적으로 분리되어 있다.

---

# 💡 다른 시스템과 관계

| 연관 시스템 | 관계 방향 | 설명 |
|---|---|---|
| Player | 진입 → 자원 투입 | Drop Zone 진입 시 백팩 자원 자동 차감 |
| Player | 진입 → 완제품 픽업 | 완제품 버퍼 Zone 진입 시 쟁반에 자동 적재 |
| MiningWorker | 채굴 → 버퍼 | 채굴 완료 자원이 자원 버퍼로 자동 전달 |
| SalesZone | 완제품 공급 | 완제품 버퍼 Zone에서 판매 인부가 완제품 픽업 |
| GameBalanceData | 데이터 조회 | 버퍼 MAX, 생산 시간 참조 |

---

# 💡 기능 명세 및 상세 규칙

## Zone 구성

| Zone | 역할 |
|---|---|
| 자원 투입 Drop Zone | 플레이어 백팩 자원 → 자원 버퍼로 이동 |
| 자원 버퍼 | 생산 대기 자원 보관 (MAX 상한 있음) |
| 컨베이어/생산 기계 | 자원 → 완제품 자동 생산 |
| 완제품 버퍼 Zone | 생산된 완제품 보관 (MAX 상한 있음) |

## 자원 투입 Drop Zone

- InteractionZone 상속
- 플레이어 진입 시 백팩 slot[0] 자원 1개씩 자동 차감 (틱마다)
- 차감된 자원: 플레이어 위치 → 자원 버퍼 위치로 DOTween lerp 날아가는 연출
- 플레이어 이탈 시 즉시 중단
- 자원 버퍼 MAX 시: Zone 위에 MAX UI 표시, 추가 투입 무시
- 틱 속도: `[SerializeField]` 에디터 노출 (InteractionZone 공통)

## 자원 버퍼

- 자원 버퍼 MAX: `GameBalanceData._resourceBufferMax`
- MAX 도달 시: 자원 투입 Drop Zone MAX UI 표시, 채굴 인부 자원 전달 버림
- 생산 소비: 생산 틱마다 자원 1개 차감 (1:1 고정 비율)

## 생산 라인

- 자원 버퍼 1개 이상 시 자동 생산 시작
- 생산 시간: `GameBalanceData._productionTime` (데이터 드리븐)
- 생산 틱마다: 자원 버퍼 -1, 완제품 버퍼 +1, 생산 애니메이션 재생
- 완제품 버퍼 MAX 시: 생산 라인 정지 (애니메이션도 함께 정지)
- 완제품 버퍼에 여유 생기면 자동 재개

## 완제품 버퍼 Zone

- InteractionZone 상속
- 플레이어 진입 시 완제품 1개씩 쟁반 slot[0]에 자동 적재 (틱마다)
- 적재 연출: 완제품 버퍼 위치 → 플레이어 쟁반으로 DOTween lerp
- 쟁반 MAX 도달 시: 추가 픽업 무시 (자원 Drop Zone과 동일 패턴)
- 완제품 버퍼 MAX 시: Zone 위에 MAX UI 표시
- 판매 인부도 동일 Zone 진입 방식으로 픽업 (InteractionZone 공통 처리)

---

# 💡 데이터 설계

## 주요 수치 (GameBalanceData)

| 항목 | 키 | 기본값 |
|---|---|---|
| 자원 버퍼 MAX | _resourceBufferMax | [TBD] |
| 완제품 버퍼 MAX | _goodsBufferMax | [TBD] |
| 완제품 1개 생산 시간 | _productionTime | [TBD] |

## ProductionManager 주요 멤버

| 멤버 | 타입 | 설명 |
|---|---|---|
| _resourceBuffer | int | 현재 자원 버퍼 수량 |
| _goodsBuffer | int | 현재 완제품 버퍼 수량 |
| _isProducing | bool | 생산 라인 동작 여부 |

## static event 목록

```csharp
public static event Action OnGoodsBufferChanged;   // 완제품 버퍼 변경 시
public static event Action OnResourceBufferChanged; // 자원 버퍼 변경 시
public static event Action OnProductionStarted;     // 생산 시작 시
public static event Action OnProductionStopped;     // 생산 정지 시
```
