/// <summary>
/// Zone 위 MAX 표시 World Space UI. SetVisible(bool)로 표시/숨김 제어.
/// 소유: ResourceDropZone, GoodsPickupZone, SalesDeskZone, InventoryComponent 등
/// 의존: 없음
/// </summary>
/// 수정 로그:
/// 2026-05-17 stub → 완성 (World Space Canvas worldCamera 초기화 추가)
using UnityEngine;

public class MaxIndicatorUI : MonoBehaviour
{
    [SerializeField] private Canvas _worldCanvas;

    private void Awake()
    {
        if (_worldCanvas != null)
        {
            _worldCanvas.worldCamera = Camera.main;
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
