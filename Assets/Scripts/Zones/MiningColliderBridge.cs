/// <summary>
/// PlayerCharacter 하위 채굴 콜라이더 오브젝트에 부착.
/// MiningZone 진입 시 활성화, 이탈 시 비활성화.
/// GridCell 충돌을 MiningZone으로 전달.
/// 소유: PlayerCharacter 하위 오브젝트 (곡괭이/드릴/트랙터 각각 1개)
/// 의존: MiningZone, GridCell
/// </summary>
using UnityEngine;

public class MiningColliderBridge : MonoBehaviour
{
    // MiningZone이 OnPlayerEnter 시 자신을 주입
    private MiningZone _miningZone;

    public void SetMiningZone(MiningZone zone)
    {
        _miningZone = zone;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_miningZone == null)
        {
            return;
        }

        if (!other.TryGetComponent<GridCell>(out var cell))
        {
            return;
        }

        _miningZone.OnMiningColliderHit(cell);
    }
}
