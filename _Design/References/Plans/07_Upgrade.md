# [Prison Life] 07_Upgrade 기획서 v1.0

---

# 💡 핵심 역할

업그레이드 Zone은 플레이어가 돈을 지불하여 게임 내 각종 기능을 향상시키는 구역이다. 모두 동일한 UpgradeZone 베이스 클래스를 상속하며, 부분 지불 방식으로 동작한다.

---

# 💡 UpgradeZone 베이스 클래스

## 공통 API

```csharp
public abstract class UpgradeZone : BaseZone
{
    protected int _totalCost;      // 총 비용 (GameBalanceData에서 로드)
    protected int _remainingCost;  // 잔여 비용 (이탈 후 재진입 시 유지)
    protected bool _isCompleted;   // 업그레이드 완료 여부
    
    // BaseZone 구현
    public override void OnPlayerEnter(PlayerController player);
    public override void OnPlayerExit(PlayerController player);
    
    // 내부 Coroutine (OnPlayerEnter에서 시작, OnPlayerExit에서 중단)
    private IEnumerator TickCoroutine(PlayerController player);
    
    // 하위 클래스 구현
    protected abstract void OnUpgradeCompleted();
}
```

## 공통 동작 규칙

- 초기 상태: `SetActive(false)` (완전 숨김)
- 진입: 허용, 틱에서 조건 미충족 시 무시
- 틱 처리:
  1. 플레이어 백팩 slot[1] (돈) 확인
  2. 돈 있으면: 백팩 slot[1] -1, 잔여 비용 -1, DOTween 빨려들어가는 연출
  3. 잔여 비용 == 0: 업그레이드 완료 처리
  4. 돈 없으면: 무시
- 이탈: Coroutine 중단, 잔여 비용 유지
- 재진입: 잔여 비용에서 차감 재개
- 완료 후: `_isCompleted = true`, 이후 틱 무시
- 틱 차감 속도: `[SerializeField] float _tickInterval` (에디터 노출)

## Zone UI

- 아이콘: 다음 업그레이드 내용 표시
- 잔여 비용: TMP로 Zone 위에 표시 (부분 지불 시 실시간 갱신)
- 완료 시: Zone `SetActive(false)`, 다음 단계 Zone `SetActive(true)`

---

# 💡 업그레이드 목록

## 1. 채굴 방식 업그레이드 (MiningUpgradeZone)

| 단계 | 표시 아이콘 | 비용 | 활성화 조건 |
|---|---|---|---|
| 곡괭이 → 드릴 | 드릴 | GameBalanceData._drillUpgradeCost | 첫 판매 완료 + 컷씬 |
| 드릴 → 트랙터 | 트랙터 | GameBalanceData._tractorUpgradeCost | 드릴 업그레이드 완료 |

- 위치: 채굴 Zone 입구 근처 고정
- 단계 전환: 동일 위치에서 아이콘/비용만 변경
- 완료 시: 플레이어 채굴 방식 즉시 전환, 백팩 자원 MAX 상향

**컷씬 연동**
- 첫 판매 완료 → `static event Action OnFirstSaleCompleted` 발행
- CameraController가 구독 → 채굴 업그레이드 Zone으로 카메라 이동 컷씬 재생
- 컷씬 종료 후 MiningUpgradeZone `SetActive(true)`

## 2. 채굴 인부 고용 (MiningWorkerHireZone)

| 항목 | 값 |
|---|---|
| 고용 인원 | 3명 |
| 비용 | GameBalanceData._miningWorkerCost |
| 활성화 조건 | 드릴 업그레이드 Zone 활성화 동시 |
| 스폰 위치 | 업그레이드 Zone 근처 (채굴 그리드 입구) |

- 완료 시: MiningWorker 3명 스폰, FSM 자동 시작

## 3. 판매 인부 고용 (SalesWorkerHireZone)

| 항목 | 값 |
|---|---|
| 고용 인원 | 1명 |
| 비용 | GameBalanceData._salesWorkerCost |
| 활성화 조건 | 채굴 인부 고용 Zone 활성화 동시 |
| 스폰 위치 | 판매 인부 업그레이드 Zone 위치 (판매 트리거 뒤) |

- 부분 지불 가능 (프로그레스바 방식으로 잔여 비용 표시)
- 완료 시: SalesWorker 1명 스폰, 동작 루프 자동 시작

## 4. 감옥 확장 (PrisonExpandZone)

| 항목 | 값 |
|---|---|
| 비용 | GameBalanceData._prisonExpandCost |
| 활성화 조건 | 감옥 수용 20명 만원 + 컷씬 |

**컷씬 연동**
- `static event Action OnPrisonFull` 발행
- CameraController가 구독 → 감옥 확장 Zone으로 카메라 이동 컷씬 재생
- 컷씬 종료 후 PrisonExpandZone `SetActive(true)`
- 완료 시: 감옥 새 구역 추가, 한도 40명으로 갱신, 게임 종료 처리

---

# 💡 업그레이드 활성화 순서 (전체 흐름)

```
게임 시작
  └─ 채굴 Zone: 채굴 가능 (곡괭이)
  └─ 튜토리얼 화살표 1번 (채굴 Zone)

첫 판매 완료
  └─ 컷씬: 채굴 업그레이드 Zone으로 카메라 이동
  └─ SetActive(true): 드릴 업그레이드 Zone
  └─ SetActive(true): 채굴 인부 고용 Zone (동시)
  └─ SetActive(true): 판매 인부 고용 Zone (동시)

드릴 업그레이드 완료
  └─ 동일 위치 Zone: 트랙터 업그레이드 Zone으로 교체

감옥 20명 만원
  └─ 컷씬: 감옥 확장 Zone으로 카메라 이동
  └─ SetActive(true): 감옥 확장 Zone

감옥 확장 완료
  └─ 게임 종료
```
