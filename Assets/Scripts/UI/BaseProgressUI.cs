/// <summary>
/// World Space 프로그레스바 + 텍스트 갱신 공통 베이스.
/// 소유: PrisonerBubbleUI, UpgradeProgressUI (상속)
/// 의존: TextMeshPro, UnityEngine.UI.Image (Filled), Camera
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseProgressUI : MonoBehaviour
{
    [SerializeField] private Canvas           _canvas;
    [SerializeField] private Image            _progressBar;  // ImageType=Filled, FillMethod=Horizontal
    [SerializeField] private TextMeshProUGUI  _label;

    protected virtual void Awake()
    {
        if (_canvas != null)
        {
            _canvas.worldCamera = Camera.main;
        }
    }

    /// <summary>프로그레스바 fillAmount와 텍스트를 한 번에 갱신.</summary>
    protected void SetProgress(int remaining, int total)
    {
        if (_progressBar != null)
        {
            // 소비될수록 채워짐: (total - remaining) / total
            _progressBar.fillAmount = total > 0 ? (float)(total - remaining) / total : 0f;
        }

        if (_label != null)
        {
            _label.text = remaining.ToString();
        }
    }

    /// <summary>프로그레스바와 텍스트 표시 여부 일괄 전환.</summary>
    protected void SetProgressVisible(bool visible)
    {
        if (_progressBar != null)
        {
            _progressBar.gameObject.SetActive(visible);
        }

        if (_label != null)
        {
            _label.gameObject.SetActive(visible);
        }
    }
}
