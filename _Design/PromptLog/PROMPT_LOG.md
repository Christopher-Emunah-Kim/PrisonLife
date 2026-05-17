# AI 프롬프트 사용 기록
> 슈퍼센트 [메타옵스] 클라이언트 개발자 과제 전형 제출용
> 사용 AI: Claude Code (Anthropic)
> 기록 방식: KARVIS가 시니의 요청마다 자동 append

---

## 사용 목적 요약

본 과제 제작 과정에서 AI(Claude Code)를 아래 목적으로 활용하였습니다.

- 게임 시스템 아키텍처 설계 보조
- Unity C# 코드 생성 및 자동 검증
- 기획서 기반 구현 범위 분해 (PLAN 스킬)
- 버그 분석 및 수정 (DEBUG 스킬)
- 코드 리뷰 (SR 스킬)

---

## 로그

<!-- 최신 요청이 위 -->

### [N] 2026-05-17 19:34
**카테고리:** CODE
**요청:**
SpawnRoutine의 WaitForSeconds를 캐싱하도록 수정해줘.

private WaitForSeconds _spawnWait;

Start 또는 Awake에서
_spawnWait = new WaitForSeconds(_prisonerData != null ? _prisonerData.spawnInterval : 3f);

SpawnRoutine에서 yield return new WaitForSeconds(...) 대신
yield return _spawnWait 사용.

동일한 패턴의 WaitForSeconds 반복 생성이 있는 다른 Coroutine도 전부 같은 방식으로 수정해줘.

---

### [N] 2026-05-17 19:50
**카테고리:** COMMIT
**요청:**
TODO랑 PROMPT_LOG 커밋하고 세션 종료할게

---

### [N] 2026-05-17 18:10
**카테고리:** CODE
**요청:**
음.. 일단 이건 다음 세션에서 할일로 남겨놔. (PlayerArrow 3D 메시 교체 관련)

---
### [N] 2026-05-17 18:00
**카테고리:** CODE
**요청:**
Canvas가 회전하는 방향이 이상한데. 내가 원한건 플레이어의 몸 주변에서 Radial로 돌면서 목표 Transform을 가리키는 화살표였어. 3D메시로 교체해서 진행해야겠다. 관련 로직 체크해봐 2. 각 Zone의 IndicatorArrow가 보이는 트리거 기준을 다시 점검해보자. 특히 GoodsPickupZone의 경우 첫 생산이 끝나면 바로 IndicatorArrow가 보여야하는데, 플레이어가 진입했을때 뜨고있어. 3. 플레이어에 붙여둔 IndicatorArrow의 경우 화면 내에 다음 목적지의 Arrow가 보이면 꺼지는것까진 잘 구현됐는데, 다시 해당 IndicatorArrow가 화면에 안보일떄 다시 표시되는 기능이 제대로 작동하질 않네. 이건 System에서 Update로 감지하게 해서 처리하는건 어때?

---

### [N] 2026-05-17 17:50
**카테고리:** CODE
**요청:**
ETutorialID를 만들어서, 이번 단계 Zone에 진입하기 전까지 IndicatorArrow를 켜두고, 해당 Zone진입할때 IndicatorArrow를 끄고, 다음 ID의 IndicatorArrow를 활성화하고, 그게 화면 밖이면 플레이어 몸주변의 IndicatorArrow_Player가 가리키도록 하는거야. Zone 진입 → 현재 화살표 OFF + 다음 화살표 ON 이 로직이니까 DropZone 진입시에 GoodsPickUp화살표도 ON하면되는거잖아? 통일해

---

### [N] 2026-05-17 17:30
**카테고리:** CODE
**요청:**
SalesZone에서 판매 버퍼 투입할때도 자원버퍼와 동일하게 PlayResourceDrop()을 호출해줘. 아니면 다른 멤버와 Play호출을 하나 더 만들거나. GameEndUI가 화면에 뜨도록 로직이 호출되고있나? PrisonExpand이후에?

---

### [N] 2026-05-17 17:15
**카테고리:** CODE
**요청:**
SFX매니저를 싱글턴으로 만들어뒀으니 mining이나 각 클립들 재생하는 로직마다 Play 호출 추가해봐. 사운드 테스트하게

---

### [N] 2026-05-17 15:50
**카테고리:** CODE
**요청:**
응 coding MODULE-14 진행해줘

---

### [N] 2026-05-17 14:46
**카테고리:** PLAN
**요청:**
MODULE-10까지 한것같고 이제 HUD랑 UI를 먼저할까. 인터페이스화살표랑. worker들은 그다음에 진행하자. 현재 UX가 잘 연결이 안되서 스토리플로우를 먼저 하는게 맞는것같아

---

### [N] 2026-05-17 14:35
**카테고리:** COMMIT
**요청:**
(세션 재개) 커밋2의 UpgradeZone들 5종 모두 커밋 분리해. → 10개 커밋 계획 "A" 승인 → 커밋 실행 요청

---

### [N] 2026-05-17 02:55
**카테고리:** CODE
**요청:**
응 잘 작동하네. 그런데 이 업그레이드 Zone들에 Prisoner의 BubbleUI처럼 돈이 사용되면, 그만큼 프로그레스바가 올라가는 연출이 포함되야해. 그럼 Canvas컴포넌트를 추가해서 처리해야하나?

---

### [N] 2026-05-17 02:40
**카테고리:** CODE
**요청:**
DOTween 연출은 지금 추가해두자. 모든 Zone공통으로 가지고있는거니 Base에 작업해.

---

### [N] 2026-05-17 02:10
**카테고리:** CODE
**요청:**
응 잘 작동하네. 그런데 이 업그레이드 Zone들에 Prisoner의 BubbleUI처럼 돈이 사용되면, 그만큼 프로그레스바가 올라가는 연출이 포함되야해. 그럼 Canvas컴포넌트를 추가해서 처리해야하나?
→ UpgradeProgressUI 별도 컴포넌트로 분리, 자식 오브젝트에 배치. 오케이 그렇게 진행하자.

---

### [N] 2026-05-17 01:20
**카테고리:** CODE
**요청:**
1. 일단 스크립트 폴더 정리를 한번 더 하자. 도메인별로 세부 카테고리를 만들어. Zones내부에도 Upgrade관련이 있고, Interaction관련이 있잖아. 2. UpgradeManager가 있어야할것같은데, 기획서상 DrillUpgradeZone이 만족해서 Drill 업그레이드가 되고나면, DrillUpgradeZone이 사라지고, 해당 트랜스폼에 TractorUpgradeZone이 등장해야해. 나머지 HireZone같은것도 금액 지불이 끝나면 사라져야하고. Manager가 오버엔지니어링인가 싶긴한데, Drill/Tractor Zone의 트랜스폼을 동일 위치로 잡을거라, 둘의 active가 현재 레벨에 맞게 제어되어야해 3. PrisonExpandZone의 경우엔 현재 PrisonFenceOriginal이라는 프리팹과 Expand됐을떄 사용할 PrisonFenceExpanded라는 빈 오브젝트를 만들어놨어. 처음엔 Original만 보이다가 확장되고나면 Expanded가 보이게 PrisonExpandZone에 멤버를 만들고 가시성을 제어하도록 로직을 추가해.

---

### [N] 2026-05-17 01:10
**카테고리:** CODE
**요청:**
MODULE 10진행하자.

---

### [N] 2026-05-17
**카테고리:** DEBUG
**요청:**
[PrisonerSpawner] SetNoCellAll(True) — 대기큐:4 이동중:2 ... 이렇게 뜨는데, NO CELL이벤트가 발생되는 순간 이미 _movingToPrison, _waitingQueue에 들어가있던 인스턴스들은 NOCELL이 떠있는데, 그 이후에 Sales가 일어나서 새롭게 _watingQueue에서 빠지고 _movingToPrison에 들어가는 인스턴스들도 DequeueFront시점에 체크해서 이동 시작후 NoCellRoot켜도록 바꿔.
---

### [N] 2026-05-17
**카테고리:** CODE
**요청:**
ProgressBar의 Image컴포넌트는 Type을 Filled로, FillMethod는vertical, bottom부터로 설정해뒀어. 지금 FillAmount하는 방식이 거꾸로야. 판매가 될수록 SetCount는 줄어드는게 맞지만, ProgressBar는 채워져야되는거잖아? 기획서에 맞게 바꿔.

---

### [N] 2026-05-17
**카테고리:** CODE
**요청:**
SalesZone의 _prisonDirection을 Vector3로 받는게 아니라 씬에 있는 오브젝트 위치를 받도록 수정하자.

---

### [N] 2026-05-16
**카테고리:** CODE
**요청:**
웅 동일 패턴 문제가 있나 Zone들 체크해봐

---

### [N] 2026-05-16
**카테고리:** CODE
**요청:**
SalesZone.SalesTick에 yield break하기전에 _tickCoroutine null처리 안해도되나? 지금 단순하게 yield break만 하고 끝내는것같은데

---

### [N] 2026-05-16
**카테고리:** CODE
**요청:**
SalesDeskZone에서 현재 진입시 쟁반 전량 책상으로 이동하게 되어있는데 ResourceDropZone과 동일 패턴으로 1개씩 이동하게 바꿔. 성공하면 _deskGoods++후 RefreshDeskMeshes(), UpdateMaxUI()호출하고, 책상 MAX면 스킵. 쟁반 비어있어도 스킵. 빠진거있나?

---


### 2026-05-15
**카테고리:** CODE
**요청:**
ConsumeResource에서 _halfFullFired 리셋하는거 이제 필요없다고했는데 다시 코드를 추가했는데, 튜토리얼 1회성 이벤트니까 플래그 원복하지마. 2. SetMeshStack에서 Instantiate반복하는데, 이거 ObjectPool로 처리 변경하자. 음. 오버엔지니어링일까? 그런데 플레이어 인벤토리에 계속 메시가 생겼다 사라져야하니까 GC부하를 생각하면 풀링을 적용하는편이 나은것같아

---

### 2026-05-15
**카테고리:** CODE
**요청:**
그럼 ResourceMesh배열 필요없어. 제거해. 여차피 동일 메시 쓸건데. 그리고 _stackBasePoint는 그냥 백팩 위치나 트레이 위치에 하나씩 두자고? 안돼. 둘이 위치가 다르잖아. 이렇게 되면 트랜스폼 배열을 만들고 0을 쟁반, 1을 백팩 1번, 2를 백팩 2번으로 해서, 자원이 0이면 백팩 1번 위치에 돈 들어가게하고, 자원이 있고, 돈도 있으면 각각 1번, 2번위치에 적층하게 해

---

### 2026-05-15
**카테고리:** CODE
**요청:**
아냐 내가 생각을 잘못한것같네. 애초에 ObjectPool<T>를 MonoBehavior로 안하고 순수 C#클래스로 변경하자. 생성자로 prefab, poolsize, 부모 transform받게하고. Zone에선 별도의 풀 컴포넌트 말고 Awake에서 직접 ObjectPool생성하도록 하는 방식은이 나을것같은데 풀 생성자에서 prefab null체크하게 해. null이면 Logger.Error출력후 생성 중단.

---

### 2026-05-15
**카테고리:** CODE
**요청:**
ResourceFlyPool.cs, GoodsFlyPool.cs 생성해줘. 각각 ObjectPool<ResourceFlyObject>, ObjectPool<GoodsFlyObject> 상속. 내용은 빈 클래스로. 인스펙터에서 할당해야하니까

---

### 2026-05-14 09:00
**카테고리:** CODE
**요청:**
1. MiningZone에 _miningMeshes GameObject 배열 SerializeField추가해. 손 소켓에 붙여둘거니까 ActivateMiningCollider에서 콜라이더와 같은 인덱스의 메시를 활성화하도록 로직을 추가해. 2. Camera가 플레이어랑 붙어있지않으니까 테스트가 불편하게. MODULE13먼저 진행해서 카메라 먼저 처리해도 빌드상 문제 없는지 확인해줘. 문제없음 MODULE13부터 P0로 올려서 처리하자. 3. GridCell의 Prefab을 생성해줘.Mesh는 비워두고 나머지 컴포넌트만 부착해. 4. MiningGridEditor.cs 수정하자. 기존 Generate 버튼 로직에 중복 생성 방지하게 생성 전 기존 자식 오브젝트 모두 삭제후 재생성하도록 하고, 3에서 만든 GridCell Prefab기반으로 생성하도록 변경해줘. 지금 Cube직접 생성방식이었던거지? Prefab으로 채워서 일괄변경 좀 편하게 할려구


---

### 2026-05-13 10:05
**카테고리:** CODE
**요청:**
TractorColliderController는 불필요해보이는걸. 삭제하고, _miningColliders[2]로 처리해. 여차피 Collider크기의 차이일뿐이지 별도로 분리할 책임을 갖고있진 않아. 그 다음에 MiningGrid 오브젝트 아래에 8*16 GridCell을 에디터 스크립트로 자동 생성해줘. 각 셀 간격은 SerializeField로 조정 가능하게. 생성 후에 MiningGrid._cells 배열에 자동 연결해줄래? 시도해보고 안되면 얘기해


---

### 2026-05-13 09:40
**카테고리:** CODE
**요청:**
오케이 검토완료. 1. InventoryComp에 방어코드에 로그 추가해. 2. 그리고 InventoryComp의 ConsumeResource에서 _halfFullFired = false리셋 로직은 제거해. 1회성 이벤트로 인디케이터 화살표 표시하려고 만든로직이 AddResource에 있는건데, 그 플래그 리셋은 불필요해.3. PlayerController는 네이밍이 헷갈려. PlayerCharacter로 변경해. 언리얼 PC랑 헷갈리니까.

---

### 2026-05-13
**카테고리:** 기타
**요청:**
왜 최근에 내가 보낸 메시지들은 PROMPT_LOG에 추가되지않았지? 규칙준수가 안되고있는것같은데, hook으로 만들어서 UserPromptSubmit으로 로그에 추가하도록 해둬

---

### 2026-05-13
**카테고리:** CODE
**요청:**
몇가지 수정할게 보이네. 1. ProductionManager.cs에 TryResumeProduction이 호출될때 코루틴을 시작했다가 완제품 버퍼가 가득 찬 상태면 바로 yield break되잖아. _goodsBuffer < _balanceData.goodsBufferMax 조건인지 추가해야될것같은데. 그리고 ProductionRoutine내에서 yield break할때 _productionCoroutine null로 안만들어도돼? 2. Util/Logger에 로깅 래퍼를 만들어놨어. 방어/에러코드에는 반드시 해당 Logger를 사용해서 로그를 찍어두도록 규칙을 수정해. 3. SalesManager봤는데, 지금 기획에서는 MoneyZone에서 MoneyManager.Add를 호출하면 onMoneyChanged가 호출되고 이게 HUD에 갱신되는 방향으로 잡아놨는데, NotifyMoneyAccumulated메서드가 왜 필요해? OnMoneyZoneAccumulated 이벤트도 필요없는것같은데, MoneyZone엔 돈이 쌓여있기만 하고, 여차피 플레이어가 트리거되면 MoneyManager를 통해 HUD갱신이 되잖아? 로직 확인해봐. 4. GameManager TriggerGameEnd()에 timeScale멈추고 나서 로고 UI띄우는 내용이 없는것같은데, 일단 OnGameEnded라는 이름으로 이벤트 만들고 OnGameEnded?.Invoke()로 발행하는걸로 수정해두자. 5. 현재까지 생성된 스크립트 파일 상단에 클래스 역할 요약 주석 추가하도록 파일 생성규칙 수정해.

---

### 2026-05-13
**카테고리:** CODE
**요청:**
GameBalanceData의 채굴 업그레이드 수치를 miningTickInterval_1/2/3, backpackResourceMax_1/2/3 개별 필드 방식 대신 MiningLevelData 구조체 List로 리팩토링해줘

---

### 2026-05-12 21:00
**카테고리:** PLAN
**요청:**
TBD 수치 확정 + 기획 이슈 5개 답변 + Module 1부터 모듈 단위로 진행 요청.
수치: miningTickInterval_1=0.5f, _2=0.33f, _3=0.17f / backpackResourceMax_1=20, _2=30, _3=60 / miningRegenTime=3f / resourceBufferMax=30 / goodsBufferMax=20 / productionTime=2f / salesDeskMax=30 / goodsPrice=10 / drillUpgradeCost=50 / tractorUpgradeCost=150 / miningWorkerCost=100 / salesWorkerCost=80 / prisonExpandCost=200 / spawnInterval=3f / _moveSpeed=5f / _rotationSpeed=10f / DOTween 날아가는=0.4f / 스케일축소=0.2f / 돈빨려들어가는=0.3f
기획이슈: 1)InventoryComp slot[0] MAX 50%↑ 시 OnResourceHalfFull 발행 2)GoodsPickupZone 최초 1개↑ 적재 시 OnGoodsPickupCompleted 발행 3)OnFirstSaleCompleted 시 드릴/채굴인부/판매인부 동시 SetActive(true) 4)트랙터 콜라이더 별도 컴포넌트 5)OnGoodsBufferChanged에 int count 파라미터 추가

---

### 2026-05-12 20:55
**카테고리:** PLAN
**요청:**
_Design/References/Plan 확인해봐. 내가 기획서를 간단히 작성해왔어. 기획서를 검토하고, PrisonLife를 구현하기 위한 단계별 구현계획을 작성해보자.

---

### 2026-05-17 20:59
**카테고리:** DEBUG
**요청:**
플레이 테스트중인데, GoodsPickupZone과 ResourceDropZone에 버퍼에 카운트는 쌓이는데, 프리팹 메시가 할당되어있지만, 쌓이는게 화면에서 보이지않아. ProductionManager랑 같이 로직 확인해서 이유가 뭔지 확인해봐. SalesZone은 정상작동해.

---
