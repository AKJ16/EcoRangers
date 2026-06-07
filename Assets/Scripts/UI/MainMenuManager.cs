using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Attributes

    [SerializeField] private RectTransform logoRect;
    [SerializeField] private Vector2 logoStartPos = new Vector2(0, 1000);
    [SerializeField] private Vector2 logoEndPos = new Vector2(0, 250);
    [SerializeField] private float logoDropDuration = 0.8f;
    [SerializeField] private CanvasGroup buttonGroup;
    [SerializeField] private float buttonFadeDuration = 0.5f;
    [SerializeField] private float delayBeforeButtons = 0.3f;
    [SerializeField] private string gameplayScene = "Gameplay";
    [SerializeField] private GameObject quitButton;

    // Methods

    private void Awake()
    {
        Time.timeScale = 1f;

        if (logoRect != null)
        {
            logoRect.anchoredPosition = logoStartPos;
        }

        if (buttonGroup != null)
        {
            buttonGroup.alpha = 0;
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

#if UNITY_WEBGL
        if (quitButton != null)
        {
            quitButton.SetActive(false);
        }
#endif
    }

    private void Start()
    {
        StartCoroutine(MenuStart());
    }

    private IEnumerator MenuStart()
    {
        float elapsed = 0f;
        while (elapsed < logoDropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / logoDropDuration;

            float s = 1.70158f;
            float t2 = t - 1.0f;
            float bounce = (t2 * t2 * ((s + 1) * t2 + s) + 1.0f);

            if (logoRect != null)
            {
                logoRect.anchoredPosition = Vector2.LerpUnclamped(logoStartPos, logoEndPos, bounce);
            }

            yield return null;
        }

        if (logoRect != null)
        {
            logoRect.anchoredPosition = logoEndPos;
        }

        yield return new WaitForSeconds(delayBeforeButtons);

        elapsed = 0f;
        while (elapsed < buttonFadeDuration)
        {
            elapsed += Time.deltaTime;
            if (buttonGroup != null)
            {
                buttonGroup.alpha = elapsed / buttonFadeDuration;
            }
            yield return null;
        }

        if (buttonGroup != null)
        {
            buttonGroup.alpha = 1f;
            buttonGroup.interactable = true;
            buttonGroup.blocksRaycasts = true;
        }
    }

    public void PlayGame()
    {
        LoadingManager.Instance.LoadScene(gameplayScene);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
