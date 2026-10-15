using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ScoreManager : MonoBehaviour
{
    // Attributes

    public static ScoreManager instance;

    [SerializeField] private int currScore = 0;
    private int combo = 0;
    private int multiplier = 1;
    [SerializeField] private int comboStep = 3;
    [SerializeField] private int maxMult = 5;
    [SerializeField] private int scoreSubtract = 10;

    [SerializeField] private GameObject comboPopupPrefab;
    [SerializeField] private Transform popupParent;
    [SerializeField] private Transform popupSpawnPoint;

    [SerializeField] private TrashSpawner trashSpawner;
    [SerializeField] private float objDuration = 1f;
    [SerializeField] private AudioClip highscoreSound;

    [Header("Highscore")]
    [SerializeField] private UIRainbow highscoreRainbow;
    [SerializeField] private UIPop newHighscorePop;
    [SerializeField] private TextMeshProUGUI newHighscoreText;
    private int startingHighscore = 0;
    private int highscore = 0;
    private bool isNewHighscore = false;
    private Coroutine activePulseCoroutine;

    [Header("First Milestone")]
    [SerializeField] private float firstMilestoneScore = 200;
    [SerializeField] private List<GameObject> firstMilestoneObjects;
    [SerializeField] private float firstMilestonePos = 3f;
    private bool firstMilestoneReached = false;

    [Header("Second Milestone")]
    [SerializeField] private float secondMilestoneScore = 500;
    [SerializeField] private List<GameObject> secondMilestoneObjects;
    [SerializeField] private float secondMilestonePos1 = 3f;
    [SerializeField] private float secondMilestonePos2 = 2.4f;
    private bool secondMilestoneReached = false;

    [Header("Third Milestone")]
    [SerializeField] private float thirdMilestoneScore = 500;
    [SerializeField] private List<GameObject> thirdMilestoneObjects;
    [SerializeField] private float thirdMilestonePos = 3f;
    private bool thirdMilestoneReached = false;

    [Header("Fourth Milestone")]
    [SerializeField] private float fourthMilestoneScore = 500;
    [SerializeField] private List<GameObject> fourthMilestoneObjects;
    [SerializeField] private float fourthMilestonePos = 3f;
    private bool fourthMilestoneReached = false;

    [Header("fifth Milestone")]
    [SerializeField] private float fifthMilestoneScore = 500;
    private bool fifthMilestoneReached = false;

    [Header("end Milestone")]
    [SerializeField] private float endMilestone1Score = 500;
    [SerializeField] private float endMilestone2Score = 500;
    private bool endMilestone1Reached = false;
    private bool endMilestone2Reached = false;

    // Methods

    private void Awake()
    {
        instance = this;
        highscore = SaveSystem.LoadHighscore();
        startingHighscore = highscore;
    }

    public void AddScore(int baseScore) 
    {
        combo++;
        int oldMult = multiplier;
        int finalScore = baseScore * multiplier;
        currScore += finalScore;

        if (currScore > highscore)
        {
            if (!isNewHighscore)
            {
                isNewHighscore = true;
                if (activePulseCoroutine != null)
                {
                    StopCoroutine(activePulseCoroutine);
                }
                activePulseCoroutine = StartCoroutine(highscorePulse());
            }
            highscore = currScore;
        }

        multiplier = Mathf.Clamp(1 + (combo / comboStep), 1, maxMult);
        SpawnPopUp(finalScore);
        if (multiplier > oldMult)
        {
            UIManager.Instance.PulseMultiplier();
        }
        UIManager.Instance.CorrectScoreFeedback();

        if (!firstMilestoneReached && currScore >= firstMilestoneScore)
        {
            firstMilestoneReached = true;
            FirstMilestone();
        }

        if (!secondMilestoneReached && currScore >= secondMilestoneScore)
        {
            secondMilestoneReached = true;
            SecondMilestone();
        }

        if (!thirdMilestoneReached && currScore >= thirdMilestoneScore)
        {
            thirdMilestoneReached = true;
            ThirdMilestone();
        }

        if (!fourthMilestoneReached && currScore >= fourthMilestoneScore)
        {
            fourthMilestoneReached = true;
            FourthMilestone();
        }

        if (!fifthMilestoneReached && currScore >= fifthMilestoneScore)
        {
            fifthMilestoneReached = true;
            FifthMilestone();
        }

        if (!endMilestone1Reached && currScore >= endMilestone1Score)
        {
           endMilestone1Reached = true;
            EndMilestone();
        }

        if (!endMilestone2Reached && currScore >= endMilestone2Score)
        {
            endMilestone2Reached = true;
            EndMilestone();
        }
    }

    public void SubtractScore()
    {
        currScore -= scoreSubtract;
        if (currScore <= 0)
        {
            currScore = 0;
        }

        if (isNewHighscore)
        {
            if (currScore <= startingHighscore)
            {
                isNewHighscore = false;
                highscore = startingHighscore;

                if (highscoreRainbow != null)
                {
                    highscoreRainbow.SetActive(false);
                }

                if (newHighscorePop != null)
                {
                    newHighscorePop.Pop(false);
                }

                if (activePulseCoroutine != null)
                {
                    StopCoroutine(activePulseCoroutine);
                    activePulseCoroutine = null;
                }
            }
            else
            {
                highscore = currScore;
            }
        }

        ResetCombo();
        UIManager.Instance.WrongScoreFeedback();
    }

    public void ResetCombo()
    {
        combo = 0;
        multiplier = 1;
    }

    public int GetScore()
    {
        return currScore;
    }

    public int GetMult()
    {
        return multiplier;
    }

    public bool CheckIsNewHighscore()
    {
        return isNewHighscore;
    }

    public int GetHighscore()
    {
        return highscore;
    }

    void SpawnPopUp(int score)
    {
        GameObject obj = Instantiate(comboPopupPrefab, popupSpawnPoint.position, Quaternion.identity, popupParent);
        ComboPopUp popup = obj.GetComponent<ComboPopUp>();
        popup.Setup(combo);
    }

    private IEnumerator highscorePulse()
    {
        if (AudioManager.Instance != null && highscoreSound != null)
        {
            AudioManager.Instance.PlaySFX(highscoreSound);
        }

        if (highscoreRainbow != null)
        {
            highscoreRainbow.SetActive(true);
        }

        if (newHighscoreText != null)
        {
            newHighscorePop.Pop(true);
        }

        yield return new WaitForSecondsRealtime(5f);

        highscoreRainbow.SetActive(false);
        newHighscorePop.Pop(false);
    }

    private void FirstMilestone()
    {
        StartCoroutine(FirstMilestoneRoutine());

        if (trashSpawner != null)
        {
            trashSpawner.FirstMilestone();
        }

        TutorialManager.Instance.ShowTutorial(1);
        Debug.Log("Milestone 1 reached!");
    }

    private IEnumerator FirstMilestoneRoutine()
    {
        if (firstMilestoneObjects.Count > 0 && firstMilestoneObjects[0] != null)
        {
            GameObject firstObj = firstMilestoneObjects[0];

            Vector3 firstTargetPos = new Vector3(firstObj.transform.position.x + firstMilestonePos, firstObj.transform.position.y, firstObj.transform.position.z);

            yield return StartCoroutine(MoveObjectRoutine(firstObj, firstTargetPos, objDuration));
        }

        for (int i = 1; i < firstMilestoneObjects.Count; i++)
        {
            var obj = firstMilestoneObjects[i];

            if (obj == null)
                continue;

            Vector3 targetPos = new Vector3(obj.transform.position.x - firstMilestonePos, obj.transform.position.y, obj.transform.position.z);

            StartCoroutine(MoveObjectRoutine(obj, targetPos, objDuration));
        }
    }

    private void SecondMilestone()
    {
        StartCoroutine(SecondMilestoneRoutine());

        if (trashSpawner != null)
        {
            trashSpawner.SecondMilestone();
        }

        TutorialManager.Instance.ShowTutorial(2);
        Debug.Log("Milestone 2 reached!");
    }

    private IEnumerator SecondMilestoneRoutine()
    {
        List<Coroutine> firstMoves = new List<Coroutine>();

        for (int i = 0; i <= 1; i++)
        {
            if (secondMilestoneObjects[i] == null)
                continue;

            GameObject obj = secondMilestoneObjects[i];

            Vector3 targetPos = new Vector3(obj.transform.position.x + secondMilestonePos1, obj.transform.position.y, obj.transform.position.z);

            StartCoroutine(MoveObjectRoutine(obj, targetPos, objDuration));
        }

        yield return new WaitForSeconds(objDuration);

        if (secondMilestoneObjects[2] != null)
        {
            GameObject obj = secondMilestoneObjects[2];

            Vector3 targetPos = new Vector3(obj.transform.position.x, obj.transform.position.y - secondMilestonePos2, obj.transform.position.z);

            yield return StartCoroutine(MoveObjectRoutine(obj, targetPos, objDuration));
        }

        for (int i = 3; i <= 6; i++)
        {
            if (secondMilestoneObjects[i] == null)
                continue;

            GameObject obj = secondMilestoneObjects[i];

            Vector3 targetPos = obj.transform.position;

            if (i == 3 || i == 4)
            {
                targetPos = new Vector3(obj.transform.position.x - secondMilestonePos1, obj.transform.position.y, obj.transform.position.z);
            }
            else
            {
                targetPos = new Vector3(obj.transform.position.x, obj.transform.position.y + secondMilestonePos1, obj.transform.position.z);
            }

            StartCoroutine(MoveObjectRoutine(obj, targetPos, objDuration));
        }
    }

    private void ThirdMilestone()
    {
        StartCoroutine(ThirdMilestoneRoutine());

        if (trashSpawner != null)
        {
            trashSpawner.ThirdMilestone();
        }

        TutorialManager.Instance.ShowTutorial(3);
        Debug.Log("Milestone 3 reached!");
    }

    private IEnumerator ThirdMilestoneRoutine()
    {
        foreach (var obj in thirdMilestoneObjects)
        {
            Vector3 targetPos = new Vector3(obj.transform.position.x, obj.transform.position.y + thirdMilestonePos, obj.transform.position.z);
            StartCoroutine(MoveObjectRoutine(obj, targetPos, objDuration));
        }
        yield return null;
    }

    private void FourthMilestone()
    {
        StartCoroutine(FourthMilestoneRoutine());

        if (trashSpawner != null)
        {
            trashSpawner.FourthMilestone();
        }

        TutorialManager.Instance.ShowTutorial(4);
        Debug.Log("Milestone 4 reached!");
    }

    private IEnumerator FourthMilestoneRoutine()
    {
        foreach (var obj in fourthMilestoneObjects)
        {
            Vector3 targetPos = new Vector3(obj.transform.position.x, obj.transform.position.y + fourthMilestonePos, obj.transform.position.z);
            StartCoroutine(MoveObjectRoutine(obj, targetPos, objDuration));
        }
        yield return null;
    }

    private void FifthMilestone()
    {
        if (trashSpawner != null)
        {
            trashSpawner.FifthMilestone();
        }

        TutorialManager.Instance.ShowTutorial(5);
        Debug.Log("Milestone 5 reached!");
    }

    private void EndMilestone()
    {
        if (trashSpawner != null)
        {
            trashSpawner.EndMilestone();
        }

        Debug.Log("Next Milestone reached!");
    }

    private IEnumerator MoveObjectRoutine(GameObject obj, Vector3 targetPos, float duration)
    {
        if (obj == null) yield break;

        Vector3 startPos = obj.transform.position;
        Vector3 delta = targetPos - startPos;

        // Detect if moving Up or Left (new items entering the screen)
        bool isMovingUpOrLeft = (delta.y > 0.05f) || (delta.x < -0.05f);
        BinCheck bin = obj.GetComponent<BinCheck>() ?? obj.GetComponentInChildren<BinCheck>();
        bool isBin = bin != null || obj.name.ToLower().Contains("bin");

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>() ?? obj.GetComponentInChildren<SpriteRenderer>();

        // 1. Edge Pop VFX: Fires right as the bin begins emerging from offscreen
        if (isBin && isMovingUpOrLeft)
        {
            SimpleVFX.BinPop(startPos, 0.85f, sr, 25);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Use unscaledDeltaTime so milestone tutorial slow-motion doesn't freeze the slide
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Bouncy overshoot pop for entering bins; smooth ease for existing bins sliding aside
            float curve = (isBin && isMovingUpOrLeft) ? EaseOutBack(t) : SmoothStep(t);
            obj.transform.position = Vector3.LerpUnclamped(startPos, targetPos, curve);

            yield return null;
        }

        obj.transform.position = targetPos;

        // 2. Arrival Pop VFX & bin bounce when it snaps into place
        if (isBin && isMovingUpOrLeft)
        {
            SimpleVFX.BinPop(targetPos, 1.25f, sr, 25);

            if (bin != null)
            {
                bin.Pulse();
            }
        }
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float m = x - 1f;
        return 1f + c3 * m * m * m + c1 * m * m;
    }

    private static float SmoothStep(float x)
    {
        return x * x * (3f - 2f * x);
    }
}
