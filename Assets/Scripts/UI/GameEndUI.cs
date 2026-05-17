/// <summary>
/// 감옥 확장 완료 시 표시되는 게임 종료 화면. DOTween 페이드인 후 시간 정지.
/// 소유: GameEnd Canvas GameObject
/// 의존: PrisonManager, DOTween
/// </summary>
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameEndUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float       _fadeDuration = 0.8f;

    private void Awake()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }
        // Canvas 자체는 활성 유지 — alpha=0으로 숨김. SetActive(false)하면 OnEnable 구독 불가.
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        if (PrisonManager.Instance != null)
        {
            PrisonManager.Instance.OnPrisonExpanded += HandlePrisonExpanded;
        }
    }

    private void OnDisable()
    {
        if (PrisonManager.Instance != null)
        {
            PrisonManager.Instance.OnPrisonExpanded -= HandlePrisonExpanded;
        }
    }

    private void HandlePrisonExpanded()
    {
        Show();
    }

    private void Show()
    {
        if (_canvasGroup == null)
        {
            Time.timeScale = 0f;
            return;
        }

        _canvasGroup.interactable   = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 0f;
        // SetUpdate(true) — timeScale=0 이후에도 트윈 동작
        _canvasGroup.DOFade(1f, _fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => Time.timeScale = 0f);
    }
}
