/// <summary>
/// Zone 위 MAX 표시 UI. SetVisible(bool)로 표시/숨김 제어.
/// 소유: ResourceDropZone, GoodsPickupZone, SalesDeskZone 등
/// 의존: 없음 (MODULE-14에서 완성 예정)
/// </summary>
using UnityEngine;

public class MaxIndicatorUI : MonoBehaviour
{
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
