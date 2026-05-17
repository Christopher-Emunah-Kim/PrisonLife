/// <summary>
/// 감옥 Zone 입구 벽면 World Space 카운터. "현재/한도" 형식, 만원 시 빨간색.
/// 소유: 감옥 Zone 벽면 GameObject
/// 의존: PrisonManager
/// </summary>
using TMPro;
using UnityEngine;

public class PrisonCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private Color           _normalColor  = Color.white;
    [SerializeField] private Color           _fullColor    = Color.red;

    [Header("World Space Canvas 카메라 참조")]
    [SerializeField] private Canvas _worldCanvas;

    private void Awake()
    {
        if (_worldCanvas != null)
        {
            _worldCanvas.worldCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        PrisonManager.Instance.OnPrisonCountChanged += HandlePrisonCountChanged;
    }

    private void OnDisable()
    {
        PrisonManager.Instance.OnPrisonCountChanged -= HandlePrisonCountChanged;
    }

    private void Start()
    {
        // 초기값 표시
        if (PrisonManager.Instance != null)
        {
            Refresh(PrisonManager.Instance.CurrentCount, PrisonManager.Instance.Capacity);
        }
    }

    private void HandlePrisonCountChanged(int current)
    {
        if (PrisonManager.Instance == null)
        {
            return;
        }
        Refresh(current, PrisonManager.Instance.Capacity);
    }

    private void Refresh(int current, int capacity)
    {
        if (_counterText == null)
        {
            return;
        }
        _counterText.text  = $"{current}/{capacity}";
        _counterText.color = (current >= capacity) ? _fullColor : _normalColor;
    }
}
