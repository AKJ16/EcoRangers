using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    // Attributes

    public static LoadingManager Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Transition Durations")]
    [SerializeField] private float fadeInDuration = 0.3f;       // Fade to black/loading screen
    [SerializeField] private float fastFadeOutDuration = 0.22f; // Fast fade out to reveal scene

    [Header("Loading Visuals")]
    [SerializeField] private GameObject[] trashPrefabs;
    [SerializeField] private Transform trashContainer;
    [SerializeField] private float spinSpeed = 200f;

    private GameObject currTrash;
    private Coroutine fadeRoutine;
    private bool isLoadingSequence = false;

    // Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Start fully covered so the initial game launch doesn't snap
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        // Smoothly and quickly fade out on initial game launch
        FadeScreenOut(fastFadeOutDuration);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Safeguard: If a scene is loaded outside of LoadSequence, fade out fast
        if (!isLoadingSequence)
        {
            FadeScreenOut(fastFadeOutDuration);
        }
    }

    private void Update()
    {
        if (currTrash != null)
        {
            currTrash.transform.Rotate(Vector3.forward, spinSpeed * Time.unscaledDeltaTime);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSequence(sceneName));
    }

    private IEnumerator LoadSequence(string sceneName)
    {
        isLoadingSequence = true;
        Time.timeScale = 1f;

        if (progressBar != null)
        {
            progressBar.value = 0f;
        }

        SpawnRandomTrash();

        // 1. Fade to black / loading overlay
        canvasGroup.blocksRaycasts = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeVolumeOut(fadeInDuration);
        }

        yield return StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 1f, fadeInDuration));

        // 2. Load the scene in the background
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float targetProgress = 0f;
        while (!operation.isDone)
        {
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
            {
                progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.unscaledDeltaTime * 2.5f);
            }

            if (operation.progress >= 0.9f && progressBar.value >= 0.98f)
            {
                progressBar.value = 1f;
            }

            if (progressText != null)
            {
                progressText.text = $"{(progressBar.value * 100):0}%";
            }

            // Ready to activate scene
            if (operation.progress >= 0.9f && progressBar.value >= 1f)
            {
                yield return new WaitForSecondsRealtime(0.15f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (currTrash != null)
        {
            Destroy(currTrash);
        }

        // 3. Really fast fade out to reveal the new scene
        yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fastFadeOutDuration));

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        isLoadingSequence = false;
    }

    public void FadeScreenOut(float duration)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutRoutine(duration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        if (canvasGroup == null) yield break;

        yield return StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 0f, duration));
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        fadeRoutine = null;
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.01f));
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private void SpawnRandomTrash()
    {
        if (trashPrefabs == null || trashPrefabs.Length == 0 || trashContainer == null) return;

        if (currTrash != null)
        {
            Destroy(currTrash);
        }

        int index = Random.Range(0, trashPrefabs.Length);
        currTrash = Instantiate(trashPrefabs[index], trashContainer);

        currTrash.transform.localPosition = Vector3.zero;
        currTrash.transform.localScale = Vector3.one;

        if (currTrash.TryGetComponent(out DragDrop dd)) dd.enabled = false;
        if (currTrash.TryGetComponent(out TrashItem ti)) ti.enabled = false;
    }
}