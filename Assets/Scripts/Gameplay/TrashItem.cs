using System.Collections;
using UnityEngine;

public class TrashItem : MonoBehaviour
{
    // Attributes

    private SpriteRenderer spriteRenderer;
    private bool isMoving = true;
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private float deleteHeight = 6f;
    [SerializeField] private int itemScore = 10;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float pulseAmount = 1.2f;

    // Methods

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isMoving && Time.timeScale > 0.01f)
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
        }

        if (transform.position.y > deleteHeight)
        {
            TrashSpawner.instance.TrashRemoved(false);
            ScoreManager.instance.ResetCombo();
            Destroy(gameObject);
        }
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    public int GetScore()
    {
        return itemScore;
    }

    public void ProcessSprite(Sprite newSprite, Vector3 newScale, float newRotation)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = newSprite;
            transform.localScale = newScale;
            transform.rotation = Quaternion.Euler(0, 0, newRotation);
        }
    }

    public void ItemFeedback()
    {
        StartCoroutine(FeedbackRoutine());
    }

    private IEnumerator FeedbackRoutine()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Vector3 originalScale = transform.localScale;
        Color originalColor = spriteRenderer.color;
        Color flashColor = new Color(2f, 2f, 2f, 1f);

        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            float curve = Mathf.Sin(t * Mathf.PI);

            transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseAmount, curve);
            spriteRenderer.color = Color.Lerp(originalColor, flashColor, curve);

            yield return null;
        }

        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
    }
}
