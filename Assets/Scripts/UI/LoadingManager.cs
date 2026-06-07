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
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private GameObject[] trashPrefabs;
    [SerializeField] private Transform trashContainer;
    [SerializeField] private float spinSpeed = 200f;

    private GameObject currTrash;

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

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
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
        Time.timeScale = 1f;
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }

        SpawnRandomTrash();

        float elapsed = 0f;
        canvasGroup.blocksRaycasts = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeVolumeOut(fadeDuration);
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = elapsed / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float targetProgress = 0f;
        while (!operation.isDone)
        {
            targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
            {
                progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.unscaledDeltaTime * 2f);
            }

            if (operation.progress >= 0.9f && progressBar.value >= 0.98f)
            {
                progressBar.value = 1f;
            }

            if (progressText != null)
            {
                progressText.text = $"{(progressBar.value * 100):0}%";
            }

            if (operation.progress >= 0.9f && progressBar.value >= 1f)
            {
                yield return new WaitForSecondsRealtime(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        if (currTrash != null) Destroy(currTrash);
    }

    private void SpawnRandomTrash()
    {
        if (trashPrefabs.Length == 0 || trashContainer == null) return;

        if (currTrash != null)
        {
            Destroy(currTrash);
        }

        int index = Random.Range(0, trashPrefabs.Length);
        currTrash = Instantiate(trashPrefabs[index], trashContainer);

        currTrash.transform.localPosition = Vector3.zero;
        currTrash.transform.localScale = Vector3.one;

        if (currTrash.TryGetComponent(out DragDrop dd))
        {
            dd.enabled = false;
        }

        if (currTrash.TryGetComponent(out TrashItem ti))
        {
            ti.enabled = false;
        }
    }
}
