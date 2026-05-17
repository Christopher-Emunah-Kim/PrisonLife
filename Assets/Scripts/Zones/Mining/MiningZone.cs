/// <summary>
/// 채굴 구역 트리거. 플레이어 진입 시 PlayerCharacter 하위 채굴 콜라이더를 단계에 맞게 활성화.
/// 채굴 콜라이더(MiningColliderBridge)는 PlayerCharacter 하위에 위치하며 이동과 함께 움직임.
/// 소유: 씬 MiningZone 오브젝트 (고정 구역)
/// 의존: BaseZone, MiningGrid, MiningColliderBridge, InventoryComponent
/// </summary>
using UnityEngine;

public class MiningZone : BaseZone
{
    [SerializeField] private MiningGrid _miningGrid;

    // 손 소켓에 부착된 단계별 채굴 도구 메시 (index 0=곡괭이, 1=드릴, 2=트랙터)
    // MiningZone 참조를 Inspector에서 연결 — PlayerCharacter 하위 손 소켓 오브젝트
    [SerializeField] private GameObject[] _miningMeshes;

    private int             _miningLevel;
    private PlayerCharacter _currentPlayer;

    // ── Zone 진입/이탈 ────────────────────────────────────

    public override void OnPlayerEnter(PlayerCharacter player)
    {
        base.OnPlayerEnter(player);
        _currentPlayer = player;
        // 진입 시점에 현재 레벨 기준 MAX 적용 — 업그레이드 후 재진입 시 자동 갱신
        player.Inventory.InitMaxValues(_miningLevel);
        ActivateMiningCollider(player, true);
    }

    public override void OnPlayerExit(PlayerCharacter player)
    {
        base.OnPlayerExit(player);
        ActivateMiningCollider(player, false);
        _currentPlayer = null;
    }

    // ── 채굴 단계 업그레이드 ─────────────────────────────

    /// <summary>DrillUpgradeZone / TractorUpgradeZone 완료 시 호출.</summary>
    public void UpgradeMiningLevel(int newLevel)
    {
        _miningLevel = newLevel;

        // 플레이어가 Zone 안에 있으면 콜라이더 즉시 전환
        if (_currentPlayer != null)
        {
            ActivateMiningCollider(_currentPlayer, true);
        }
        // MAX 갱신은 다음 MiningZone 진입 시점에 적용 (OnPlayerEnter)
    }

    // ── GridCell 충돌 처리 ─────────────────────────────────

    /// <summary>MiningColliderBridge(PlayerCharacter 하위)에서 GridCell 충돌 시 호출.</summary>
    public void OnMiningColliderHit(GridCell cell)
    {
        if (_currentPlayer == null)
        {
            return;
        }

        if (_currentPlayer.Inventory.IsResourceFull())
        {
            return;
        }

        _miningGrid.MineCell(cell);
        _currentPlayer.Inventory.AddResource(1);
    }

    // ── 내부 헬퍼 ─────────────────────────────────────────

    private void ActivateMiningCollider(PlayerCharacter player, bool active)
    {
        MiningColliderBridge[] bridges = player.GetComponentsInChildren<MiningColliderBridge>(true);

        if (bridges.Length == 0)
        {
            Logger.Warn("MiningZone", "PlayerCharacter 하위에 MiningColliderBridge가 없음");
            return;
        }

        for (int i = 0; i < bridges.Length; i++)
        {
            bool isCurrentLevel = active && (i == _miningLevel);
            bridges[i].gameObject.SetActive(isCurrentLevel);

            if (isCurrentLevel)
            {
                // 현재 단계 브릿지에 MiningZone 참조 주입
                bridges[i].SetMiningZone(this);
            }
        }

        // 동일 인덱스의 도구 메시도 함께 전환
        if (_miningMeshes == null)
        {
            return;
        }

        for (int i = 0; i < _miningMeshes.Length; i++)
        {
            if (_miningMeshes[i] == null)
            {
                continue;
            }

            _miningMeshes[i].SetActive(active && i == _miningLevel);
        }
    }
}
