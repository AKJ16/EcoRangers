using TMPro;
using UnityEngine;

public class UIRainbow : MonoBehaviour
{
    // Attributes

    [SerializeField] UIPulse pulse;
    [SerializeField] private float colorSpeed = 2f;
    [SerializeField] private bool initiallyActive = false;

    private TextMeshProUGUI text;
    private Color originalColor;
    private bool isActive = false;

    // Methods

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalColor = text.color;
        isActive = initiallyActive;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            text.color = originalColor;
        }
    }

    private void Update()
    {
        if (!isActive)
        {
            if (pulse != null && pulse.enabled)
            {
                pulse.enabled = false;
            }
            return;
        }

        if (pulse != null && !pulse.enabled)
        {
            pulse.enabled = true;
        }

        float hue = Mathf.Repeat(Time.unscaledTime * colorSpeed, 1f);
        text.color = Color.HSVToRGB(hue, 0.7f, 1f);
    }
}
