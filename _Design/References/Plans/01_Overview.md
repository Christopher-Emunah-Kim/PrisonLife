# [Prison Life] 플레이어블 광고 기획서 v1.0

## 문서 이력

| 버전 | 수정일 | 변경 요약 |
|---|---|---|
| v1.0 | 2026-05-12 | 최초 작성 |

---

# 💡 게임 개요

## 기본 스펙

| 항목 | 값 |
|---|---|
| 게임 장르 | 자원 채취 → 생산 → 판매 시뮬레이터 (플레이어블 광고) |
| 유니티 버전 | 2022.3.62f2 |
| 해상도 | 720 x 1280 (Portrait 고정) |
| 빌드 타겟 | Android |
| 씬 구성 | 단일 씬 |
| 카메라 방식 | 쿼터뷰 Perspective, 플레이어 즉시 추적 |
| 카메라 각도 | Rotation X: 45도, Y: 45도 (에디터 노출) |
| Safe Area | HUD 전체 Safe Area 기준 배치 |
| 화면 회전 | Portrait 고정 (auto-rotate 비활성) |
| 뒤로가기 버튼 | 무시 (후순위 처리) |
| 스토어 링크 | [STORE_URL] |
| 게임 시작 시퀀스 | 씬 로드 후 즉시 플레이 가능 |
| Input System | New Input System 패키지 |
| 외부 패키지 | DOTween, TextMeshPro (TMP) |

---

# 💡 Zone 배치 및 역할 정의

## 맵 구성 (쿼터뷰 기준 시계 방향)

| Zone | 위치 | 역할 |
|---|---|---|
| MiningZone | 1시 방향 | 석탄 채굴. 8×16 그리드 |
| ProductionZone | 11시 방향 | 자원 투입 → 완제품 생산 |
| GoodsBufferZone | 9시 방향 (생산 라인 출구) | 완제품 보관 |
| SalesZone | 7시 방향 | 완제품 판매, 죄수 큐 대기 |
| MoneyZone | 판매 Zone 우측 | 판매금 수령 |
| PrisonZone | 5시 방향 | 죄수 수용 (초기 한도 20명) |
| 플레이어 시작 위치 | 중앙 | - |

## Zone 물리적 간격 규칙

Zone 간 거리를 충분히 이격하여 플레이어가 동시에 2개 Zone Collider에 걸치는 상황을 원천 차단한다.

---

# 💡 아키텍처 결정 사항

## 클래스 구조

**BaseCharacter (공통 베이스)**
- 포함 대상: Player, MiningWorker, SalesWorker
- 공통 멤버: 이동 속도, lerp 회전 속도 (`[SerializeField]` 에디터 노출)
- 죄수(Prisoner)는 AI 없는 단순 이동 오브젝트로 BaseCharacter 제외

**BaseZone (공통 베이스)**
- 모든 Zone이 상속
- 공통 API: `OnPlayerEnter(PlayerController)`, `OnPlayerExit(PlayerController)`
- 내부 틱 처리: Coroutine으로 자체 관리 (`OnTriggerStay` 미사용)
- `OnTriggerEnter` → Coroutine 시작, `OnTriggerExit` → Coroutine 중단

**InteractionZone : BaseZone**
- Drop/Pickup 동작 Zone 공통 베이스
- 자원 투입, 완제품 픽업, 판매금 수령, 판매 책상 내려놓기 공통 처리
- 틱 속도: `[SerializeField]` 에디터 노출

**UpgradeZone : BaseZone**
- 모든 업그레이드 Zone 공통 베이스
- 부분 지불 가능: 잔액 유지 (이탈 후 재진입 시 잔액에서 차감 재개)
- 돈 부족 시: 진입 허용, 틱 조건 미충족으로 무시
- 잔액이 0이 되면 업그레이드 완료, 이후 틱 무시
- 업그레이드 완료 시 `SetActive(false)` → 다음 단계 Zone `SetActive(true)`
- 틱 차감 속도: `const float` (`[SerializeField]` 에디터 노출)

## Zone 로직 소유권

Zone이 `IInteractableZone` 인터페이스 보유. Player가 인터페이스에 의존.

```csharp
public interface IInteractableZone
{
    void OnPlayerEnter(PlayerController player);
    void OnPlayerExit(PlayerController player);
}
```

## 시스템 통신 방식

C# `static event Action` + Manager 싱글톤 조합.
`OnDestroy`에서 반드시 구독 해제 (기획서 전체 공통 규칙).

```csharp
// 예시
public static event Action<int> OnGoodsProduced;
public static event Action OnResourceMaxReached;
```

## 인벤토리 구조

**백팩 (Player 전용)**
- `slot[0]` = 자원 (석탄), 별도 MAX
- `slot[1]` = 돈, 별도 MAX
- 메시 표시: 5개 단위당 더미 메시 1개 추가
- 두 슬롯 모두 있을 때: 플레이어→자원→돈 순 적층
- 돈만 있을 때: 플레이어에 바로 붙어서 표시

**쟁반 (완제품 Carry)**
- `slot[0]` = 완제품, MAX = 백팩과 동일 크기
- 메시 표시: 백팩과 동일하게 5개 단위당 더미 메시 1개

## 오브젝트 풀링 대상

| 대상 | 초기 풀 사이즈 |
|---|---|
| 죄수(Prisoner) | 30 |
| 채굴 자원 (날아가는 연출) | 30 |
| 돈 메시 | 30 |
| 완제품 메시 | 30 |

## Physics Layer 설계

| 레이어 | 충돌 대상 |
|---|---|
| Player | Zone, MiningGrid |
| Worker | Zone, MiningGrid |
| Prisoner | Zone (감옥 입장 트리거만) |
| Zone | Player, Worker, Prisoner |
| MiningGrid | Player, Worker |
| Projectile | MiningGrid (채굴 자원 날아가는 연출) |

## ScriptableObject 분리

**GameBalanceData (SO 1번)**
- 밸런스 수치 전체 (채굴/생산/판매/업그레이드 비용/버퍼 MAX 등)
- 상세 스키마: `09_Data.md` 참조

**PrisonerData (SO 2번)**
- 죄수 행동 수치 (스폰 간격, 큐 최대 인원, 구매 수량 범위)
- 상세 스키마: `09_Data.md` 참조

## 에디터 노출 ([SerializeField]) 항목

| 항목 | 위치 |
|---|---|
| 카메라 오프셋, 컷씬 복귀 시간(0.8초), lerp 속도 | CameraController |
| BaseCharacter 이동 속도, lerp 회전 속도 | BaseCharacter |
| 조이스틱 데드존 (기본값 0.1f) | JoystickController |
| Drop/Pickup 틱 속도 (기본값: 틱당 1개) | InteractionZone |
| 업그레이드 Zone 틱 차감 속도 | UpgradeZone |
| DOTween 연출 duration 전체 | 각 컴포넌트 |
| 인디케이터 화살표 보빙 진폭/속도 | IndicatorArrow |
| 오브젝트 풀 초기 사이즈 (기본값 30) | ObjectPool |
| 트랙터 삽 콜라이더 크기 | TractorController |
| 판매 체크 틱 간격 (기본값 2초) | SalesZone |
| 메시 표시 단위 (기본값 5) | InventoryComponent |
| 컷씬 lerp 속도 | CameraController |

---

# 💡 UI 전체 구성

| 위치 | 요소 | 설명 |
|---|---|---|
| 좌상단 | 게임 로고 + PLAY 버튼 | 상시 표시. 탭 시 [STORE_URL] 이동 |
| 우상단 | 돈 아이콘 + TMP 소지금 | HUD 소지금 = 백팩 slot[1] 수량 |
| 좌하단 | 음소거 버튼 | 초기 상태: 소리 켜짐 |
| 하단 50% | 동적 조이스틱 | Safe Area 제외 하단 50% 어디서나 터치 가능 |

---

# 💡 인디케이터 화살표 시스템

## 두 가지 표시 방식

| 타입 | 방식 | 조건 |
|---|---|---|
| Type 1 | Zone 머리 위 고정 화살표 (World Space) | 대상 Zone이 카메라 시야 내에 있을 때 |
| Type 2 | 플레이어 몸 주변 방향 화살표 | 대상 Zone이 카메라 시야 밖에 있을 때 |

- 전환: 대상 Zone이 시야에 들어오면 Type 2 사라지고 Type 1만 표시
- Type 1 화살표: 위아래 보빙 연출 (`[SerializeField]` 진폭/속도)
- 소거: 최초 Zone 진입 시 영구 제거 (bool 플래그)

## 튜토리얼 화살표 발동 순서

| 순서 | 발동 조건 | 가리키는 Zone |
|---|---|---|
| 1 | 게임 시작 | 채굴 Zone |
| 2 | 자원 절반 이상 획득 | 자원 투입 Drop Zone |
| 3 | 자원 투입 완료 | 완제품 버퍼 Zone |
| 4 | 완제품 픽업 완료 | 판매 책상 Zone |
| 5 | 판매 완료 | 판매금 수령 Zone |

- 이후 업그레이드 Zone 활성화는 카메라 컷씬으로만 안내 (화살표 없음)

---

# 💡 컷씬 시스템

| 트리거 | 컷씬 내용 |
|---|---|
| 첫 판매 완료 | 카메라가 채굴 업그레이드 Zone으로 이동 후 복귀 |
| 죄수 수용 Zone 20명 만원 | 카메라가 감옥 확장 업그레이드 Zone으로 이동 후 복귀 |

- 컷씬 중 플레이어 입력 차단
- 게임 로직(인부, 생산 라인, 죄수 이동)은 컷씬 중에도 계속 동작
- 컷씬 종료 후 0.8초간 플레이어 위치로 카메라 복귀 (Coroutine lerp)
- 두 컷씬은 이벤트 순서상 동시 발생 불가 → 중복 방지 로직 불필요

---

# 💡 게임 종료 처리

- 감옥 확장 업그레이드 완료 시 게임 일시 정지
- 화면 중앙에 게임 로고 표시
- 로고 탭 시 [STORE_URL] 이동

---

# 💡 시스템 파일 목록

| 파일 | 내용 |
|---|---|
| 01_Overview.md | 본 문서 |
| 02_Player.md | 이동, 조이스틱, 백팩/쟁반 인벤토리 |
| 03_Zone_Mining.md | 채굴 그리드, 업그레이드 3단계, 채굴 인부 FSM |
| 04_Zone_Production.md | 자원 투입, 생산 라인, 완제품 버퍼 |
| 05_Zone_Sales.md | 판매 책상, 죄수 큐, 판매금 Zone, 판매 인부 |
| 06_Zone_Prison.md | 감옥 수용, 확장 업그레이드, 죄수 풀링 |
| 07_Upgrade.md | UpgradeZone 베이스 클래스, 각 업그레이드 정의 |
| 08_UI.md | HUD, 인디케이터, MAX UI, 컷씬 |
| 09_Data.md | ScriptableObject 스키마 2종, Physics Layer, Pool |
