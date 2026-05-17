/// <summary>
/// Screen Space HUD. 소지금·PLAY버튼·음소거 담당. Safe Area 적용.
/// 소유: HUD Canvas GameObject
/// 의존: MoneyManager, SFXManager
/// </summary>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("소지금")]
    [SerializeField] private TextMeshProUGUI _moneyText;

    [Header("PLAY 버튼")]
    [SerializeField] private Button _playButton;
    [SerializeField] private string _storeUrl = "https://play.google.com/store";

    [Header("음소거 버튼")]
    [SerializeField] private Button   _muteButton;
    [SerializeField] private Image    _muteIcon;
    [SerializeField] private Sprite   _soundOnSprite;
    [SerializeField] private Sprite   _soundOffSprite;

    [Header("Safe Area 패널")]
    [SerializeField] private RectTransform _safeAreaPanel;

    private bool _isMuted;

    private void Awake()
    {
        ApplySafeArea();
    }

    private void OnEnable()
    {
        MoneyManager.OnMoneyChanged += HandleMoneyChanged;
    }

    private void OnDisable()
    {
        MoneyManager.OnMoneyChanged -= HandleMoneyChanged;
    }

    private void Start()
    {
        _playButton.onClick.AddListener(OnPlayButtonClicked);
        _muteButton.onClick.AddListener(OnMuteButtonClicked);

        // 초기 소지금 표시
        if (MoneyManager.Instance != null)
        {
            UpdateMoneyText(MoneyManager.Instance.CurrentMoney);
        }

        RefreshMuteIcon();
    }

    private void HandleMoneyChanged(int amount)
    {
        UpdateMoneyText(amount);
    }

    private void UpdateMoneyText(int amount)
    {
        if (_moneyText != null)
        {
            _moneyText.text = amount.ToString();
        }
    }

    private void OnPlayButtonClicked()
    {
        Application.OpenURL(_storeUrl);
    }

    private void OnMuteButtonClicked()
    {
        _isMuted = !_isMuted;
        SFXManager.Instance?.SetMute(_isMuted);
        RefreshMuteIcon();
    }

    private void RefreshMuteIcon()
    {
        if (_muteIcon == null)
        {
            return;
        }
        _muteIcon.sprite = _isMuted ? _soundOffSprite : _soundOnSprite;
    }

    private void ApplySafeArea()
    {
        if (_safeAreaPanel == null)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _safeAreaPanel.anchorMin = anchorMin;
        _safeAreaPanel.anchorMax = anchorMax;
    }
}
