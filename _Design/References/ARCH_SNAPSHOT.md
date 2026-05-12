# ARCH_SNAPSHOT
> PrisonLife Unity 프로젝트 구현 스냅샷. INIT마다 로드.
> 최초 작성: 2026-05-12 | 업데이트: 코드 완료 시마다

## PROJECT_INFO
```yaml
engine:    Unity 2022.3.62f2
language:  C#
platform:  Android (Portrait 고정)
packages:  DOTween, TextMeshPro, New Input System
```

## CLASS_REGISTRY
> 현재 구현된 클래스 목록. 신규 클래스 추가 시 갱신.

| 클래스명 | 경로 | 책임 | 상태 |
|---|---|---|---|
| (미구현) | - | - | - |

## INTEGRATION_MAP
> 시스템 간 연결 지점. 신규 연결 추가 시 갱신.

| From | To | 방식 | 설명 |
|---|---|---|---|
| (미구현) | - | - | - |

## PATTERNS
> 이 프로젝트에서 확립된 코딩 방식.

```
Zone 틱:      OnTriggerEnter → StartCoroutine(_tickCoroutine) / OnTriggerExit → StopCoroutine
이벤트:       static event Action + Manager 싱글톤 / OnEnable 구독 / OnDisable 해제
인벤토리:     int[] 슬롯 배열 + [SerializeField] MAX + 5개 단위 더미 메시
업그레이드:   UpgradeZone 베이스 클래스 상속 / 잔액 유지 / SetActive(false) 초기화
풀링:         ObjectPool<T> / 초기 사이즈 30 / Initialize() 리셋 / Return() 반환
SO 참조:      GameBalanceData (밸런스) / PrisonerData (죄수 행동) / Inspector 주입
```

## ARCHITECTURE_DECISIONS
> 01_Overview.md에서 확정된 아키텍처 결정 사항 요약.

```
Zone 로직 소유권:    Zone이 IInteractableZone 보유, Player가 인터페이스에 의존
인벤토리 표현:       숫자 + 더미 메시 (5개 단위)
인부 이동:           Transform 직선 이동 (NavMesh 미사용)
시스템 통신:         C# static event Action + Manager 싱글톤
카메라 컷씬:         Coroutine lerp, 별도 시스템 없음
UpgradeZone:         베이스 클래스 상속, OnEnter/OnExit 공통 API
버퍼 상한:           자원투입/완제품/판매책상 모두 MAX 있음, MAX UI 표시
인디케이터:          World Space
채굴 그리드:         8×16, 리젠 시간 데이터 드리븐
Physics Layer:       Player/Worker/Prisoner/Zone/MiningGrid/Projectile 분리
```
