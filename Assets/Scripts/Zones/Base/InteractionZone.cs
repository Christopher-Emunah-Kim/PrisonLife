/// <summary>
/// 틱 기반 상호작용 Zone 공통 베이스. DropTick/PickupTick 로직은 하위 클래스가 구현.
/// 소유: ResourceDropZone, GoodsPickupZone, SalesDeskZone 등 (상속)
/// 의존: BaseZone, PlayerCharacter
/// </summary>
/// 수정 로그:
/// 2026-05-17 WaitForSeconds 캐싱 (_tickWait)
/// 2026-05-17 Awake virtual 선언 — 하위 클래스 base.Awake() 누락으로 _tickWait null 버그 수정
using System.Collections;
using UnityEngine;

public abstract class InteractionZone : BaseZone
{
    [SerializeField] protected float _tickInterval = 0.66f;

    private WaitForSeconds _tickWait;

    protected virtual void Awake()
    {
        _tickWait = new WaitForSeconds(_tickInterval);
    }

    // 하위 클래스에서 Awake override 시 반드시 base.Awake() 호출 필요 — _tickWait 초기화 누락 방지

    public override void OnPlayerEnter(PlayerCharacter player)
    {
        base.OnPlayerEnter(player);
        StopTick(); // 이중 시작 방지
        
        _tickCoroutine = StartCoroutine(TickRoutine(player));
    }

    public override void OnPlayerExit(PlayerCharacter player)
    {
        base.OnPlayerExit(player); // StopTick 포함
    }

    private IEnumerator TickRoutine(PlayerCharacter player)
    {
        while (true)
        {
            yield return _tickWait;

            if (player == null)
            {
                Logger.Warn("InteractionZone", "Tick 중단: player가 null");
                _tickCoroutine = null;
                yield break;
            }

            OnTick(player);
        }
    }

    // 하위 클래스에서 실제 틱 로직 구현
    protected abstract void OnTick(PlayerCharacter player);
}
