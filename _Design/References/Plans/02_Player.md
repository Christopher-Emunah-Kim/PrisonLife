# [Prison Life] 02_Player 기획서 v1.0

---

# 💡 핵심 역할

플레이어는 조이스틱 입력으로 이동하며, 각 Zone에 진입하여 자원 채굴 → 생산 투입 → 완제품 픽업 → 판매 → 판매금 수령 → 업그레이드의 게임 루프를 수행한다.

---

# 💡 다른 시스템과 관계

| 연관 시스템 | 관계 방향 | 설명 |
|---|---|---|
| JoystickController | 입력 → Player | 터치 입력 델타로 이동 방향 전달 |
| BaseZone (전체) | Zone → Player | Zone이 IInteractableZone 인터페이스로 Player에 이벤트 전달 |
| InventoryComponent | Player → Inventory | 자원/돈/완제품 수량 관리 |
| CameraController | Camera → Player | 카메라가 플레이어 위치 즉시 추적 |

---

# 💡 기능 명세 및 상세 규칙

## 조이스틱 입력

- New Input System 사용
- 동적 조이스틱: 화면 하단 50% 영역 (Safe Area 제외) 어디서나 터치 시 해당 위치에 조이스틱 생성
- UI 버튼(음소거, PLAY)과 조이스틱은 별도 터치 포인트로 처리 (멀티터치 독립)
- 데드존: `[SerializeField] float _deadzone = 0.1f` (전체 반경 대비 10%)
- 데드존 미만 입력은 이동 무시

**조이스틱 활성화 흐름**
1. 하단 50% 터치 시작 → 해당 위치에 조이스틱 UI 생성
2. 터치 드래그 → 방향/크기 계산 → 이동 입력 전달
3. 터치 종료 → 조이스틱 UI 비활성화, 이동 정지

## 플레이어 이동

- BaseCharacter 상속
- 이동 속도: `[SerializeField] float _moveSpeed` (에디터 노출)
- 회전 방식: 조이스틱 입력 방향으로 lerp 회전
- 회전 속도: `[SerializeField] float _rotationSpeed` (에디터 노출)
- 컷씬 중 입력 차단: `bool _inputBlocked` 플래그로 이동 처리 스킵

## 인벤토리 (InventoryComponent)

**백팩 슬롯 구조**

| 슬롯 | 내용 | MAX |
|---|---|---|
| slot[0] | 자원 (석탄) | GameBalanceData 참조 (단계별 상향) |
| slot[1] | 돈 | GameBalanceData 참조 |

- 슬롯별 독립 MAX
- MAX 도달 시 플레이어 머리 위 MAX UI 표시 (MaxIndicatorUI 컴포넌트)
- MAX 상태에서 추가 획득 시 무시

**쟁반 슬롯 구조**

| 슬롯 | 내용 | MAX |
|---|---|---|
| slot[0] | 완제품 (수갑) | 백팩 MAX와 동일 크기 |

**더미 메시 표시 규칙**

- 5개 단위당 더미 메시 1개 추가/제거
- `[SerializeField] int _meshUnit = 5` (에디터 노출)
- 백팩 자원+돈 모두 있을 때: 플레이어→자원 메시→돈 메시 순 적층
- 돈만 있을 때: 플레이어에 바로 붙어서 표시

**HUD 소지금 연동**

- HUD 소지금 = 백팩 slot[1] 수량과 항상 동기화
- `static event Action<int> OnMoneyChanged` 로 UI에 전달

## Zone 진입/이탈

- `OnTriggerEnter` → `IInteractableZone.OnPlayerEnter(this)` 호출
- `OnTriggerExit` → `IInteractableZone.OnPlayerExit(this)` 호출
- 동시에 2개 Zone에 걸치는 상황 없음 (Zone 간 거리 이격으로 원천 차단)

## 게임 시작 연출

- 씬 로드 후 즉시 플레이 가능
- 인피니티 심볼 조작 유도 연출 표시
- 첫 터치 입력 시 인피니티 심볼 비활성화
- 채굴 Zone 위에 튜토리얼 Type 1 화살표 표시

---

# 💡 데이터 설계

## InventoryComponent 주요 멤버

| 멤버 | 타입 | 설명 |
|---|---|---|
| _backpackSlots | int[2] | slot[0]=자원, slot[1]=돈 |
| _backpackMax | int[2] | 슬롯별 최대치 (GameBalanceData에서 로드) |
| _traySlots | int[1] | slot[0]=완제품 |
| _trayMax | int | 백팩 MAX와 동일 |
| _meshUnit | int | 더미 메시 표시 단위 (SerializeField, 기본값 5) |

## BaseCharacter 주요 멤버

| 멤버 | 타입 | 설명 |
|---|---|---|
| _moveSpeed | float | SerializeField, 에디터 노출 |
| _rotationSpeed | float | SerializeField, 에디터 노출 |
| _inputBlocked | bool | 컷씬 중 입력 차단 플래그 |
