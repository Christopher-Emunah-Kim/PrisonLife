/// <summary>
/// 모든 Zone의 공통 기반 클래스. LayerMask 필터 + Coroutine 생명주기 관리.
/// 소유: 각 Zone (상속)
/// 의존: IInteractableZone, PlayerCharacter
/// </summary>
using UnityEngine;

public abstract class BaseZone : MonoBehaviour, IInteractableZone
{
    // Inspector에서 Player 레이어 설정 필수
    [SerializeField] protected LayerMask _playerLayer;

    protected Coroutine _tickCoroutine;

    // ── IInteractableZone 구현 ────────────────────────────

    public virtual void OnPlayerEnter(PlayerCharacter player)
    {
        // 하위 클래스에서 Coroutine 시작
    }

    public virtual void OnPlayerExit(PlayerCharacter player)
    {
        StopTick();
    }

    // ── Collider 진입/이탈 → LayerMask 필터링 ────────────

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerLayer(other))
        {
            return;
        }

        if (!other.TryGetComponent<PlayerCharacter>(out var player))
        {
            return;
        }

        OnPlayerEnter(player);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!IsPlayerLayer(other))
        {
            return;
        }

        if (!other.TryGetComponent<PlayerCharacter>(out var player))
        {
            return;
        }

        OnPlayerExit(player);
    }

    // ── 헬퍼 ─────────────────────────────────────────────

    protected void StopTick()
    {
        if (_tickCoroutine != null)
        {
            StopCoroutine(_tickCoroutine);
            _tickCoroutine = null;
        }
    }

    private bool IsPlayerLayer(Collider other)
    {
        return (_playerLayer.value & (1 << other.gameObject.layer)) != 0;
    }
}
