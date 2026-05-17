/// <summary>
/// 죄수 머리 위 말풍선 UI. 구매 수량 TMP + 프로그레스바 + "No Cell!" 표시.
/// 소유: Prisoner (SerializeField 참조)
/// 의존: BaseProgressUI, TextMeshPro, UnityEngine.UI.Image (Filled)
/// </summary>
using UnityEngine;

public class PrisonerBubbleUI : BaseProgressUI
{
    [SerializeField] private GameObject _noCellRoot;

    private int _maxCount;

    /// <summary>스폰 시 초기 구매 수량 설정. 프로그레스바 100%로 초기화.</summary>
    public void SetCount(int count)
    {
        _maxCount = count;
        SetProgress(count, count);

        if (_noCellRoot != null)
        {
            _noCellRoot.SetActive(false);
        }
    }

    /// <summary>판매 1회 완료시마다 호출. 남은 수량으로 프로그레스바 갱신.</summary>
    public void SetProgressCount(int remaining)
    {
        SetProgress(remaining, _maxCount);
    }

    public void ShowNoCell(bool show)
    {
        if (_noCellRoot != null)
        {
            _noCellRoot.SetActive(show);
        }

        SetProgressVisible(!show);
    }
}
