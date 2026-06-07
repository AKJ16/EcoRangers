using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Attributes

    public static UIManager Instance;

    public TimerUI timerUI;
    public ScoreUI scoreUI;
    public MultiplierUI multiplierUI;

    // Methods

    private void Awake()
    {
        Instance = this;
    }

    public void AddTimeFeedback()
    {
        timerUI.AddTime();
    }

    public void CorrectScoreFeedback()
    {
        scoreUI.CorrectFeedback();
    }

    public void WrongScoreFeedback()
    {
        scoreUI.WrongFeedback();
    }

    public void PulseMultiplier()
    {
        multiplierUI.Pulse();
    }
}
