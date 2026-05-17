/// <summary>
/// UpgradeZone 위에 표시되는 World Space 프로그레스바 UI.
/// Zone의 자식 오브젝트로 배치. UpgradeZone.OnCostChanged()에서 호출됨.
/// 소유: 각 UpgradeZone 자식 오브젝트
/// 의존: TextMeshPro, UnityEngine.UI.Image (Filled), Camera
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeProgressUI : MonoBehaviour
{
    [SerializeField] private Canvas           _canvas;
    [SerializeField] private Image            _progressBar;  // ImageType=Filled, FillMethod=Horizontal
    [SerializeField] private TextMeshProUGUI  _costText;     // "잔여비용" 표시

    private void Awake()
    {
        if (_canvas != null)
        {
            _canvas.worldCamera = Camera.main;
        }
    }

    /// <summary>UpgradeZone.Start()에서 총 비용 확정 시 초기화.</summary>
    public void Initialize(int totalCost)
    {
        UpdateProgress(totalCost, totalCost);
    }

    /// <summary>돈 차감 성공 시마다 호출. 프로그레스바와 텍스트 갱신.</summary>
    public void UpdateProgress(int remaining, int total)
    {
        if (_progressBar != null)
        {
            // 지불할수록 채워짐: (total - remaining) / total
            _progressBar.fillAmount = total > 0 ? (float)(total - remaining) / total : 0f;
        }

        if (_costText != null)
        {
            _costText.text = remaining.ToString();
        }
    }
}
