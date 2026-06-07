using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    // Attribute
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.3f;
    [SerializeField] private AudioClip tickSound;

    public GameTimer timer;
    public Slider slider;
    private float maxTime;
    private bool pulsingHalf = false;
    private bool pulsingTen = false;
    private Coroutine pulseRoutine;
    private Vector3 originalScale;

    // Methods

    private void Awake()
    {
        maxTime = timer.GetMaxTime();
        slider.maxValue = maxTime;
        originalScale = slider.transform.localScale;
    }

    private void Update()
    {
        float time = timer.GetTime();
        slider.value = time;

        if (time <= (maxTime / 2) && !pulsingHalf)
        {
            StartPulse(Color.yellow);
            pulsingHalf = true;

            if (AudioManager.Instance != null && tickSound != null)
            {
                AudioManager.Instance.PlaySFX(tickSound);
            }
        }
        
        if (time > maxTime / 2)
        {
            pulsingHalf = false;
        }

        if (time <= 10f && !pulsingTen)
        {
            pulsingTen = true;
            StartPulse(Color.red, true);
        }

        if (time > 10f)
        {
            pulsingTen = false;
            StopPulseAndReset();
        }
    }

    public void AddTime()
    {
        float time = timer.GetTime();
        if (time <= 10f)
        {
            return;
        }
        StartPulse(Color.yellow, false);
    }

    private void StartPulse(Color color, bool continuous = false)
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }
        pulseRoutine = StartCoroutine(Pulse(color, continuous));
    }

    private void StopPulseAndReset()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }
        slider.transform.localScale = originalScale;
        slider.fillRect.GetComponent<Image>().color = Color.yellow;
    }

    private IEnumerator Pulse(Color color, bool continuous)
    {
        Image fill = slider.fillRect.GetComponent<Image>();
        fill.color = color;

        do
        {
            if (continuous && AudioManager.Instance != null && tickSound != null)
            {
                AudioManager.Instance.PlaySFX(tickSound);
            }

            float t = 0f;
            while (t < pulseDuration)
            {
                t += Time.deltaTime;
                float ping = Mathf.Sin((t / pulseDuration) * Mathf.PI);
                Vector3 newScale = new Vector3(originalScale.x, Mathf.Lerp(originalScale.y, originalScale.y * pulseScale, ping), originalScale.z);
                slider.transform.localScale = newScale;                
                yield return null;
            }
            slider.transform.localScale = originalScale;

            if (continuous)
            {
                yield return new WaitForSeconds(1f - pulseDuration);
            }
        } while (continuous);
    }
}