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

<!-- KARVIS: 새 요청은 아래에 추가. CLAUDE.md PROMPT_LOGGING 섹션 형식 준수 -->

### [14] 2026-05-14 09:30
**카테고리:** COMMIT
**요청:**
나중에 하자. 일단 여기까지 하고 커밋하자. 이번에 에셋 추가해둔게 좀 많아. 커밋 계획잡기위해 변경사항 모두 확인하고, 커밋계획잡아봐.

---

### [13] 2026-05-14 09:00
**카테고리:** CODE
**요청:**
1. MiningZone에 _miningMeshes GameObject 배열 SerializeField추가해. 손 소켓에 붙여둘거니까 ActivateMiningCollider에서 콜라이더와 같은 인덱스의 메시를 활성화하도록 로직을 추가해. 2. Camera가 플레이어랑 붙어있지않으니까 테스트가 불편하게. MODULE13먼저 진행해서 카메라 먼저 처리해도 빌드상 문제 없는지 확인해줘. 문제없음 MODULE13부터 P0로 올려서 처리하자. 3. GridCell의 Prefab을 생성해줘.Mesh는 비워두고 나머지 컴포넌트만 부착해. 4. MiningGridEditor.cs 수정하자. 기존 Generate 버튼 로직에 중복 생성 방지하게 생성 전 기존 자식 오브젝트 모두 삭제후 재생성하도록 하고, 3에서 만든 GridCell Prefab기반으로 생성하도록 변경해줘. 지금 Cube직접 생성방식이었던거지? Prefab으로 채워서 일괄변경 좀 편하게 할려구

---

### [12] 2026-05-13 10:15
**카테고리:** CODE
**요청:**
채굴콜라이더를 MiningZone하위로 뒀는데, 해당 오브젝트는 PlayerCharacter 하위로 붙이려고 만드는거야? 플레이어 이동방향으로 GridCell을 감지하려면 Player하위에 있어야되지않나

---

### [11] 2026-05-13 10:05
**카테고리:** CODE
**요청:**
TractorColliderController는 불필요해보이는걸. 삭제하고, _miningColliders[2]로 처리해. 여차피 Collider크기의 차이일뿐이지 별도로 분리할 책임을 갖고있진 않아. 그 다음에 MiningGrid 오브젝트 아래에 8*16 GridCell을 에디터 스크립트로 자동 생성해줘. 각 셀 간격은 SerializeField로 조정 가능하게. 생성 후에 MiningGrid._cells 배열에 자동 연결해줄래? 시도해보고 안되면 얘기해

---

### [10] 2026-05-13 09:55
**카테고리:** CODE
**요청:**
UpgradeZone이랑InteractionZone의 _tickInterval의 차이는 왜 둔거지? 기획상에 연출 속도는 동일하게 잡았을텐데. 일단 인스펙터에서 내가 밸런싱할거니까, 놔두고 다음 작업 진행하자.

---

### [9] 2026-05-13 09:50
**카테고리:** CODE
**요청:**
문제없어 다음 작업 진행하자.

---

### [8] 2026-05-13 09:40
**카테고리:** CODE
**요청:**
오케이 검토완료. 1. InventoryComp에 방어코드에 로그 추가해. 2. 그리고 InventoryComp의 ConsumeResource에서 _halfFullFired = false리셋 로직은 제거해. 1회성 이벤트로 인디케이터 화살표 표시하려고 만든로직이 AddResource에 있는건데, 그 플래그 리셋은 불필요해.3. PlayerController는 네이밍이 헷갈려. PlayerCharacter로 변경해. 언리얼 PC랑 헷갈리니까.

---

### [7] 2026-05-13
**카테고리:** 기타
**요청:**
왜 최근에 내가 보낸 메시지들은 PROMPT_LOG에 추가되지않았지? 규칙준수가 안되고있는것같은데, hook으로 만들어서 UserPromptSubmit으로 로그에 추가하도록 해둬

---

### [6] 2026-05-13
**카테고리:** COMMIT
**요청:**
모듈 단위로 커밋 분할해.

---

### [5] 2026-05-13
**카테고리:** COMMIT
**요청:**
오케이 여기까지 오류없으니 여기서 일단 커밋하자. 수정사항 확인하고 CHANGESET 업데이트해. 커밋계획 세워봐.

---

### [4] 2026-05-13
**카테고리:** CODE
**요청:**
몇가지 수정할게 보이네. 1. ProductionManager.cs에 TryResumeProduction이 호출될때 코루틴을 시작했다가 완제품 버퍼가 가득 찬 상태면 바로 yield break되잖아. _goodsBuffer < _balanceData.goodsBufferMax 조건인지 추가해야될것같은데. 그리고 ProductionRoutine내에서 yield break할때 _productionCoroutine null로 안만들어도돼? 2. Util/Logger에 로깅 래퍼를 만들어놨어. 방어/에러코드에는 반드시 해당 Logger를 사용해서 로그를 찍어두도록 규칙을 수정해. 3. SalesManager봤는데, 지금 기획에서는 MoneyZone에서 MoneyManager.Add를 호출하면 onMoneyChanged가 호출되고 이게 HUD에 갱신되는 방향으로 잡아놨는데, NotifyMoneyAccumulated메서드가 왜 필요해? OnMoneyZoneAccumulated 이벤트도 필요없는것같은데, MoneyZone엔 돈이 쌓여있기만 하고, 여차피 플레이어가 트리거되면 MoneyManager를 통해 HUD갱신이 되잖아? 로직 확인해봐. 4. GameManager TriggerGameEnd()에 timeScale멈추고 나서 로고 UI띄우는 내용이 없는것같은데, 일단 OnGameEnded라는 이름으로 이벤트 만들고 OnGameEnded?.Invoke()로 발행하는걸로 수정해두자. 5. 현재까지 생성된 스크립트 파일 상단에 클래스 역할 요약 주석 추가하도록 파일 생성규칙 수정해.

---

### [3] 2026-05-13
**카테고리:** CODE
**요청:**
GameBalanceData의 채굴 업그레이드 수치를 miningTickInterval_1/2/3, backpackResourceMax_1/2/3 개별 필드 방식 대신 MiningLevelData 구조체 List로 리팩토링해줘

---

### [2] 2026-05-12 21:00
**카테고리:** PLAN
**요청:**
TBD 수치 확정 + 기획 이슈 5개 답변 + Module 1부터 모듈 단위로 진행 요청.
수치: miningTickInterval_1=0.5f, _2=0.33f, _3=0.17f / backpackResourceMax_1=20, _2=30, _3=60 / miningRegenTime=3f / resourceBufferMax=30 / goodsBufferMax=20 / productionTime=2f / salesDeskMax=30 / goodsPrice=10 / drillUpgradeCost=50 / tractorUpgradeCost=150 / miningWorkerCost=100 / salesWorkerCost=80 / prisonExpandCost=200 / spawnInterval=3f / _moveSpeed=5f / _rotationSpeed=10f / DOTween 날아가는=0.4f / 스케일축소=0.2f / 돈빨려들어가는=0.3f
기획이슈: 1)InventoryComp slot[0] MAX 50%↑ 시 OnResourceHalfFull 발행 2)GoodsPickupZone 최초 1개↑ 적재 시 OnGoodsPickupCompleted 발행 3)OnFirstSaleCompleted 시 드릴/채굴인부/판매인부 동시 SetActive(true) 4)트랙터 콜라이더 별도 컴포넌트 5)OnGoodsBufferChanged에 int count 파라미터 추가

---

### [1] 2026-05-12 20:55
**카테고리:** PLAN
**요청:**
_Design/References/Plan 확인해봐. 내가 기획서를 간단히 작성해왔어. 기획서를 검토하고, PrisonLife를 구현하기 위한 단계별 구현계획을 작성해보자.

---

