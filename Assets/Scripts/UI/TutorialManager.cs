using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    // Attributes

    public static TutorialManager Instance;

    [SerializeField] private UIMenuTransition transition;
    [SerializeField] private GameObject[] milestoneObjects;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private TextMeshProUGUI extraText;
    [SerializeField] private string startText = "Start!!";
    [SerializeField] private string continueText = "Got It!!";

    private bool initialTutorial = true;
    private Coroutine slowTimeCoroutine;

    // Methods

    private void Awake()
    {
        Instance = this;
        foreach (GameObject obj in milestoneObjects)
        {
            obj.SetActive(false);
        }
    }

    private void Start()
    {
        ShowTutorial(0);
    }

    public void ShowTutorial(int index) 
    {
        if (index < 0 || index >= milestoneObjects.Length) return;

        if (buttonText != null)
        {
            buttonText.text = (index == 0) ? startText : continueText;
        }

        if (extraText != null)
        {
            extraText.alpha = (index == 0) ? 0 : 1;
        }

        for (int i = 0; i < milestoneObjects.Length; i++)
        {
            milestoneObjects[i].SetActive(i == index);
        }

        AudioManager.Instance.MuffleAudio(0.6f);

        if (slowTimeCoroutine != null)
        {
            StopCoroutine(slowTimeCoroutine);
        }

        slowTimeCoroutine = StartCoroutine(SlowTime(index));
        transition.Show();
    }

    private IEnumerator SlowTime(int index)
    {
        if (index == 0)
        {
            Time.timeScale = 0f;
            yield break;
        }

        if (index > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        float elapsed = 0;
        while (elapsed < 1.0f)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1f, 0f, elapsed);
            yield return null;
        }
        Time.timeScale = 0f;
    }

    public void CloseTutorial()
    {
        if (slowTimeCoroutine != null)
        {
            StopCoroutine(slowTimeCoroutine);
            slowTimeCoroutine = null;
        }

        Time.timeScale = 1f;
        AudioManager.Instance.UnmuffleAudio(0.6f);
        transition.Hide();

        if (initialTutorial)
        {
            GameTimer.Instance.StartTimer();
            initialTutorial = false;
        }
    }
}
