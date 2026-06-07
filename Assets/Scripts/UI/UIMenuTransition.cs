using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class UIMenuTransition : MonoBehaviour
{
    // Attributes

    [SerializeField] private CanvasGroup dimOverlay; 
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private float dimDuration = 0.4f;
    [SerializeField] private float dropDuration = 0.6f;
    [SerializeField] private Vector2 startPos = new Vector2(0, 1200);
    [SerializeField] private Vector2 endPos = new Vector2(0, 0);

    [SerializeField] private AudioClip swipeSound;
    [SerializeField] private AudioClip popInSound;
    [SerializeField] private AudioClip popOutSound;

    private CanvasGroup rootCanvasGroup;

    // Methods

    private void Awake()
    {
        rootCanvasGroup = GetComponent<CanvasGroup>();
        ResetUI();
    }

    public void ResetUI()
    {
        if (dimOverlay != null)
        {
            dimOverlay.alpha = 0;
        }

        if (mainPanel != null)
        {
            mainPanel.anchoredPosition = startPos;
        }

        rootCanvasGroup.alpha = 0;
        rootCanvasGroup.interactable = false;
        rootCanvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(ShowSequence());
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(HideSequence());
    }

    private IEnumerator ShowSequence()
    {
        rootCanvasGroup.alpha = 1;
        rootCanvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < dimDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (dimOverlay != null) dimOverlay.alpha = elapsed / dimDuration;
            yield return null;
        }

        if (dimOverlay != null)
        {
            dimOverlay.alpha = 1f;
        }

        if (AudioManager.Instance != null && swipeSound != null && popInSound != null)
        {
            AudioManager.Instance.PlaySFX(swipeSound);
            AudioManager.Instance.PlaySFX(popInSound);
        }

        elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / dropDuration;

            float s = 1.70158f;
            float t2 = t - 1.0f;
            float bounce = (t2 * t2 * ((s + 1) * t2 + s) + 1.0f);

            mainPanel.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, bounce);
            yield return null;
        }

        mainPanel.anchoredPosition = endPos;
        rootCanvasGroup.interactable = true;
    }

    private IEnumerator HideSequence()
    {
        rootCanvasGroup.interactable = false;

        if (AudioManager.Instance != null && swipeSound != null && popOutSound != null)
        {
            AudioManager.Instance.PlaySFX(swipeSound);
            AudioManager.Instance.PlaySFX(popOutSound);
        }

        float elapsed = 0f;
        float duration = 0.3f;
        Vector2 currentPos = mainPanel.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            mainPanel.anchoredPosition = Vector2.Lerp(currentPos, startPos, t);
            rootCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        ResetUI();
    }
}
