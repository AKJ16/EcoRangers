using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Attributes

    [SerializeField] private UIMenuTransition transition;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    private bool isGameOver = false;

    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private GameObject trophyIcon;
    [SerializeField] private TextMeshProUGUI newHighscoreText;
    [SerializeField] private UIRainbow highscoreRainbow;
    [SerializeField] private UIRainbow newHighscoreRainbow;
    [SerializeField] private AudioClip highscoreSound;

    // Methods

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        int finalScore = ScoreManager.instance.GetScore();
        int highscore = ScoreManager.instance.GetHighscore();
        bool isNewHighscore = ScoreManager.instance.CheckIsNewHighscore();

        if (finalScoreText != null)
        {
            finalScoreText.text = finalScore.ToString();
        }

        if (highscoreText != null)
        {
            highscoreText.text = highscore.ToString();
        }

        if (isNewHighscore)
        {
            SaveSystem.SaveHighscore(finalScore);

            if (trophyIcon != null)
            {
                trophyIcon.SetActive(true);
            }

            if (newHighscoreText != null)
            {
                newHighscoreText.gameObject.SetActive(true);
            }

            if (highscoreRainbow != null)
            {
                highscoreRainbow.SetActive(true);
            }

            if (newHighscoreRainbow != null)
            {
                newHighscoreRainbow.SetActive(true);
            }
        }

        AudioManager.Instance.MuffleAudio(2.0f);
        transition.Show();
        StartCoroutine(SlowTime(highscoreSound, isNewHighscore));

    }

    private IEnumerator SlowTime(AudioClip highscoreSound, bool isNewHighscore)
    {
        float elapsed = 0;
        while (elapsed < 1.0f)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1f, 0f, elapsed);
            yield return null;
        }
        Time.timeScale = 0f;

        if (AudioManager.Instance != null && highscoreSound != null && isNewHighscore)
        {
            AudioManager.Instance.PlaySFX(highscoreSound);
        }
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        LoadingManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        LoadingManager.Instance.LoadScene("MainMenu");
    }
}
