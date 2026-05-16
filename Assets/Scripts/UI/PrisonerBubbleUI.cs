/// <summary>
/// 죄수 머리 위 말풍선 UI. 구매 수량 TMP + 프로그레스바 + "No Cell!" 표시.
/// 소유: Prisoner (SerializeField 참조)
/// 의존: TextMeshPro, UnityEngine.UI.Image (Filled)
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrisonerBubbleUI : MonoBehaviour
{
    [SerializeField] private Canvas          _canvas;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Image           _progressBar;   // ImageType=Filled, FillMethod=Horizontal
    [SerializeField] private GameObject      _noCellRoot;

    private int _maxCount;

    private void Awake()
    {
        if (_canvas != null)
        {
            _canvas.worldCamera = Camera.main;
        }
    }

    /// <summary>스폰 시 초기 구매 수량 설정. 프로그레스바 100%로 초기화.</summary>
    public void SetCount(int count)
    {
        _maxCount = count;

        if (_countText != null)
        {
            _countText.text = count.ToString();
        }

        SetProgress(count, count);

        if (_noCellRoot != null)
        {
            _noCellRoot.SetActive(false);
        }
    }

    /// <summary>판매 틱마다 호출. 남은 수량과 최대 수량으로 프로그레스바 갱신.</summary>
    public void SetProgressCount(int remaining)
    {
        if (_countText != null)
        {
            _countText.text = remaining.ToString();
        }

        SetProgress(remaining, _maxCount);
    }

    public void ShowNoCell(bool show)
    {
        if (_noCellRoot != null)
        {
            _noCellRoot.SetActive(show);
        }

        if (_countText != null)
        {
            _countText.gameObject.SetActive(!show);
        }

        if (_progressBar != null)
        {
            _progressBar.gameObject.SetActive(!show);
        }
    }

    private void SetProgress(int remaining, int max)
    {
        if (_progressBar == null)
        {
            return;
        }

        // 판매될수록 채워짐: (max - remaining) / max
        _progressBar.fillAmount = max > 0 ? (float)(max - remaining) / max : 0f;
    }
}
