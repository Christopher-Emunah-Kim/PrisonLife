/// <summary>
/// UpgradeZone 위에 표시되는 World Space 프로그레스바 UI.
/// Zone의 자식 오브젝트로 배치. UpgradeZone.OnCostChanged()에서 호출됨.
/// 소유: 각 UpgradeZone 자식 오브젝트
/// 의존: BaseProgressUI
/// </summary>
using UnityEngine;

public class UpgradeProgressUI : BaseProgressUI
{
    /// <summary>UpgradeZone.Start()에서 총 비용 확정 시 초기화.</summary>
    public void Initialize(int totalCost)
    {
        UpdateProgress(totalCost, totalCost);
    }

    /// <summary>돈 차감 성공 시마다 호출. 프로그레스바와 텍스트 갱신.</summary>
    public void UpdateProgress(int remaining, int total)
    {
        SetProgress(remaining, total);
    }
}
