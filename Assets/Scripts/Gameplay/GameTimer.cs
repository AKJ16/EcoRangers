using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    // Attributes

    public static GameTimer Instance;

    [SerializeField] private float maxTime = 60f;
    public float timeRemaining;
    private bool isRunning = true;
    public UnityEvent onTimeUp;
    private bool gameStart = false;

    // Methods

    private void Awake()
    {
        Instance = this;
        timeRemaining = maxTime;
    }

    public void StartTimer()
    {
        gameStart = true;
        isRunning = true;
    }

    private void Update()
    {
        if (!gameStart || !isRunning)
        {   
            Time.timeScale = 0;
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            Debug.Log("Time's Up!");
            onTimeUp.Invoke();
        }
    }

    public float GetTime()
    {
        return timeRemaining;
    }

    public float GetMaxTime()
    {
        return maxTime;
    }

    public void AddTimer(float addTime)
    {
        timeRemaining += addTime;
        UIManager.Instance.AddTimeFeedback();
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}
