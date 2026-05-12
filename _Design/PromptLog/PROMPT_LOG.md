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

