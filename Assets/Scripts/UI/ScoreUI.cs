using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    // Attributes

    [SerializeField] float feedbackDuration = 0.3f;
    [SerializeField] float shakeStrength = 5f;
    [SerializeField] float pulseScale = 1.1f;

    public TextMeshProUGUI scoreText;
    private Vector3 originalPos;
    private Color originalColor;
    private Vector3 originalScale;

    // Methods

    private void Awake()
    {
        originalPos = scoreText.rectTransform.localPosition;
        originalColor = scoreText.color;
        originalScale = scoreText.rectTransform.localScale;
    }

    private void Update()
    {
        scoreText.text = ScoreManager.instance.GetScore().ToString();
    }

    public void WrongFeedback()
    {
        StopAllCoroutines();
        StartCoroutine(WrongFeedbackRoutine());
    }

    private IEnumerator WrongFeedbackRoutine()
    {
        float timer = 0f;
        scoreText.color = Color.red;

        while (timer < feedbackDuration)
        {
            timer += Time.deltaTime;
            Vector2 shakeOffset = Random.insideUnitCircle * shakeStrength;
            scoreText.rectTransform.localPosition = originalPos + (Vector3)shakeOffset;
            yield return null;
        }

        scoreText.rectTransform.localPosition = originalPos;
        scoreText.color  = originalColor;
    }

    public void CorrectFeedback()
    {
        StopAllCoroutines();
        StartCoroutine(CorrectFeedbackRoutine());
    }

    private IEnumerator CorrectFeedbackRoutine()
    {
        float timer = 0f;

        while (timer < feedbackDuration)
        {
            timer += Time.deltaTime;
            float t = timer / feedbackDuration;
            scoreText.color = Color.Lerp(new Color(0.196f, 0.804f, 0.196f), originalColor, t);

            float pulse = Mathf.Sin(t * Mathf.PI);
            float scale = Mathf.Lerp(1f, pulseScale, pulse);
            scoreText.rectTransform.localScale = originalScale * scale;
            yield return null;
        }

        scoreText.color = originalColor;
    }
}
