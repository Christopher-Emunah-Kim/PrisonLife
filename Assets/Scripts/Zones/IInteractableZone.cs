/// <summary>
/// Zone-Player 상호작용 계약. BaseZone이 구현, PlayerCharacter가 의존.
/// 소유: BaseZone (상속 경유)
/// 의존: PlayerCharacter
/// </summary>
public interface IInteractableZone
{
    void OnPlayerEnter(PlayerCharacter player);
    void OnPlayerExit(PlayerCharacter player);
}
