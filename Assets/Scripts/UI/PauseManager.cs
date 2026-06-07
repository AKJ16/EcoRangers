using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // Attributes

    public static PauseManager Instance;

    [SerializeField] private UIMenuTransition transition;
    private bool isPaused = false;

    // Methods

    private void Awake()
    {
        Instance = this;
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0 && !isPaused)
        {
            PauseGame();
        }
        else if (isPaused)
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        AudioManager.Instance.MuffleAudio(0.2f);
        isPaused = true;
        transition.Show();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        AudioManager.Instance.UnmuffleAudio(0.2f);
        isPaused = false;
        transition.Hide();
        Time.timeScale = 1f;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        LoadingManager.Instance.LoadScene("MainMenu");
    }
}
