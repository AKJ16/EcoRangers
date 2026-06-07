using UnityEngine;
using UnityEngine.UI;

public class UIButtonClick : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;

    private void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null && clickSound != null)
                {
                    AudioManager.Instance.PlaySFX(clickSound);
                }
            });
        }
    }
}