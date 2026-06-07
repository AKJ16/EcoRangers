using TMPro;
using UnityEngine;

public class MultiplierUI : MonoBehaviour
{
    // Attributes

    [SerializeField] private float pulseScale = 1.0f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float lostSpeed = 5;

    public TextMeshProUGUI text;
    private Vector3 originalScale;
    private bool isPulsing;
    private bool isLost;
    private int oldMult = 1;

    // Methods

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        int mult = ScoreManager.instance.GetMult();
        if (mult <= 1)
        {
            transform.localScale = Vector3.zero;
            text.alpha = 0f;
        }
        else
        {
            transform.localScale = originalScale;
            text.alpha = 1f;
        }
    }

    private void Update()
    {
        int mult = ScoreManager.instance.GetMult();
        if (mult > oldMult)
        {
            oldMult = mult;
        }
        text.text = $"x{oldMult}";

        if (mult > 1)
        {
            text.alpha = 1.0f;
            
        }
        else
        {
            isLost = true;
        }

        if (isLost)
        {
            Vector3 targetScale = Vector3.zero;

            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, lostSpeed * Time.deltaTime);

            float scalePercent = Mathf.Clamp01(transform.localScale.x / originalScale.x);
            text.alpha = scalePercent;

            if (transform.localScale.x <= 0.05f)
            {
                transform.localScale = Vector3.zero;
                text.alpha = 0f;
                isLost = false;
                oldMult = mult;
            }

            return;
        }

        if (isPulsing)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * pulseScale, pulseSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.localScale, originalScale * pulseScale) < 0.05f)
            {
                isPulsing = false;
            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, pulseSpeed * Time.deltaTime);
        }
    }

    public void Pulse()
    {
        isPulsing = true;
    }
}
