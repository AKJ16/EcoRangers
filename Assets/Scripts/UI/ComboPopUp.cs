using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ComboPopUp : MonoBehaviour
{
    // Attributes

    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifetime = 1f;
    public TextMeshProUGUI text;
    private Color color;
    private float timer;

    // Methods

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        color = text.color;
    }

    public void Setup(int combo)
    {
        text.text = $"{combo}x COMBO!";
    }

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        timer += Time.deltaTime;

        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        text.color = new Color(color.r, color.g, color.b, alpha);

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
