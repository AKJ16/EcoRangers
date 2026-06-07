using TMPro;
using UnityEngine;

public class UIPop : MonoBehaviour
{
    // Attributes

    [SerializeField] TextMeshProUGUI text;
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private UIPulse pulseScript;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isAnimating = false;
    private bool reachOvershoot = false;

    // Methods

    private void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        if (text != null)
        {
            text.alpha = 0f;
        }
    }



    private void Update()
    {
        if (!isAnimating) return;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, speed * Time.unscaledDeltaTime);

        if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
        {
            if (reachOvershoot)
            {
                reachOvershoot = false;
                targetScale = originalScale;
            }
            else
            {
                isAnimating = false;
                if (targetScale == originalScale && pulseScript != null)
                {
                    pulseScript.enabled = true;
                }

                if (targetScale == Vector3.zero)
                {
                    if (text != null)
                    {
                        text.alpha = 0f;
                    }
                    gameObject.SetActive(false);
                }
            }
        }
    }

    public void Pop(bool show)
    {
        if (pulseScript != null)
        {
            pulseScript.enabled = false;
        }

        isAnimating = true;
        gameObject.SetActive(true);

        if (show)
        {
            if (text != null)
            {
                text.alpha = 1f;
            }
            transform.localScale = Vector3.zero;
            targetScale = originalScale * popScale;
            reachOvershoot = true;
        }
        else
        {
            targetScale = Vector3.zero;
            reachOvershoot = false;
        }
    }
}