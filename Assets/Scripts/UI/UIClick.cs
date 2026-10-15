using UnityEngine;
using UnityEngine.UI;

// Nama class HARUS sama dengan nama file (UIClick.cs), kalau tidak Unity menganggap
// komponen ini "Missing (Mono Script)" di semua tombol yang memakainya.
public class UIClick : MonoBehaviour
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