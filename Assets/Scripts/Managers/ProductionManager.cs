/// <summary>
/// 자원 버퍼 → 생산 Coroutine → 완제품 버퍼 흐름을 관리하는 생산 관리자.
/// 소유: GameManager (초기화), ResourceDropZone / GoodsPickupZone (버퍼 조작)
/// 의존: GameBalanceData, Singleton<T>
/// </summary>
/// 수정 로그:
/// 2026-05-14 ResourceBufferMax / GoodsBufferMax 프로퍼티 추가 (ResourceDropZone, GoodsPickupZone 참조)
/// 2026-05-17 생산 완료 시 SFXManager.PlayProduction() 추가
/// 2026-05-17 WaitForSeconds 캐싱 (_productionWait)
/// 2026-05-17 Singleton<T> 베이스 클래스 적용
/// 2026-05-17 ShakeEffect 연동 — 생산 시작 시 Loop(), 정지 시 Stop()
using System;
using System.Collections;
using UnityEngine;

public class ProductionManager : Singleton<ProductionManager>
{
    // int: 변경 후 버퍼 수량 (DD5 — SalesWorker 대기 판단용)
    public event Action<int> OnGoodsBufferChanged;
    public event Action<int> OnResourceBufferChanged;
    public event Action      OnProductionStarted;
    public event Action      OnProductionStopped;

    [SerializeField] private GameBalanceData _balanceData;
    [SerializeField] private ShakeEffect     _shakeEffect;      // 생산 기계 메시에 부착된 진동 컴포넌트

    private int            _resourceBuffer;
    private int            _goodsBuffer;
    private Coroutine      _productionCoroutine;
    private WaitForSeconds _productionWait;

    public int ResourceBuffer    => _resourceBuffer;
    public int ResourceBufferMax => _balanceData != null ? _balanceData.resourceBufferMax : 0;
    public int GoodsBuffer       => _goodsBuffer;
    public int GoodsBufferMax    => _balanceData != null ? _balanceData.goodsBufferMax : 0;

    protected override void OnAwake()
    {
        _productionWait = new WaitForSeconds(_balanceData != null ? _balanceData.productionTime : 1f);
    }

    // 채굴 Zone / MiningWorker가 자원 투입 시 호출
    public bool AddResource(int amount = 1)
    {
        if (_resourceBuffer >= _balanceData.resourceBufferMax)
        {
            return false;
        }

        _resourceBuffer = Mathf.Min(_resourceBuffer + amount, _balanceData.resourceBufferMax);
        OnResourceBufferChanged?.Invoke(_resourceBuffer);

        // 생산 라인이 멈춰 있고, 완제품 버퍼에 여유가 있을 때만 재시작
        if (_productionCoroutine == null && _goodsBuffer < _balanceData.goodsBufferMax)
        {
            _productionCoroutine = StartCoroutine(ProductionRoutine());
        }
        return true;
    }

    // GoodsPickupZone이 완제품 꺼낼 때 호출
    public bool ConsumeGoods(int amount = 1)
    {
        if (_goodsBuffer < amount)
        {
            Logger.Warn("ProductionManager", "ConsumeGoods 호출 시 goodsBuffer 부족");
            return false;
        }

        _goodsBuffer -= amount;
        OnGoodsBufferChanged?.Invoke(_goodsBuffer);
        TryResumeProduction();
        return true;
    }

    private IEnumerator ProductionRoutine()
    {
        OnProductionStarted?.Invoke();
        _shakeEffect?.Loop();

        while (true)
        {
            if (_resourceBuffer <= 0)
            {
                StopProduction();
                yield break;
            }

            if (_goodsBuffer >= _balanceData.goodsBufferMax)
            {
                StopProduction();
                yield break;
            }

            yield return _productionWait;

            _resourceBuffer--;
            OnResourceBufferChanged?.Invoke(_resourceBuffer);

            _goodsBuffer++;
            OnGoodsBufferChanged?.Invoke(_goodsBuffer);
            SFXManager.Instance?.PlayProduction();
        }
    }

    private void StopProduction()
    {
        _productionCoroutine = null;
        OnProductionStopped?.Invoke();
        _shakeEffect?.Stop();
    }

    // GoodsPickupZone이 완제품을 가져간 뒤 버퍼 여유 생기면 생산 재개
    public void TryResumeProduction()
    {
        if (_productionCoroutine != null)
        {
            return;
        }

        if (_resourceBuffer <= 0 || _goodsBuffer >= _balanceData.goodsBufferMax)
        {
            return;
        }

        _productionCoroutine = StartCoroutine(ProductionRoutine());
    }
}
