using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    // Attributes

    public static TutorialManager Instance;

    [SerializeField] private UIMenuTransition transition;
    [SerializeField] private GameObject[] milestoneObjects;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private TextMeshProUGUI extraText;
    [SerializeField] private string startText = "Start!!";
    [SerializeField] private string continueText = "Got It!!";

    // Polish
    [SerializeField] private float revealDelay = 0.55f;     // jeda sebelum isi panel mulai muncul
    [SerializeField] private float closeUnlockDelay = 1.1f; // panel baru bisa ditutup (Space/Enter) setelah ini
    [SerializeField] private float alertDelay = 0.9f;       // jeda sebelum tag "New Trash Type!!" muncul

    private bool initialTutorial = true;
    private Coroutine slowTimeCoroutine;
    private Coroutine alertCoroutine;
    private TutorialPanelAnimator[] animators;
    private bool isOpen;
    private float closeUnlockTime;
    private Quaternion extraRestRotation = Quaternion.identity;

    // Methods

    private void Awake()
    {
        Instance = this;

        if (extraText != null)
        {
            extraRestRotation = extraText.rectTransform.localRotation;
        }

        if (milestoneObjects == null || milestoneObjects.Length == 0)
        {
            Debug.LogError("TutorialManager: 'Milestone Objects' kosong di Inspector (harus berisi panel Milestone 0-5). Tutorial tidak akan muncul dan timer tidak akan mulai.");
            milestoneObjects = new GameObject[0];
        }

        animators = new TutorialPanelAnimator[milestoneObjects.Length];

        for (int i = 0; i < milestoneObjects.Length; i++)
        {
            GameObject obj = milestoneObjects[i];
            if (obj == null)
            {
                Debug.LogWarning("TutorialManager: milestoneObjects[" + i + "] kosong (None) di Inspector.");
                continue;
            }

            obj.SetActive(false);

            // Animator ditambahkan lewat kode, tidak perlu dipasang di scene
            if (!obj.TryGetComponent(out TutorialPanelAnimator animator))
            {
                animator = obj.AddComponent<TutorialPanelAnimator>();
            }
            animators[i] = animator;
        }
    }

    private void Start()
    {
        ShowTutorial(0);
    }

    private void Update()
    {
        // Space / Enter = tombol "Got It" (hanya setelah panel selesai turun)
        if (!isOpen || Time.unscaledTime < closeUnlockTime) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame ||
            keyboard.enterKey.wasPressedThisFrame ||
            keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            CloseTutorial();
        }
    }

    public void ShowTutorial(int index)
    {
        if (index < 0 || index >= milestoneObjects.Length) return;
        if (milestoneObjects[index] == null) return;

        if (buttonText != null)
        {
            buttonText.text = (index == 0) ? startText : continueText;
        }

        if (extraText != null)
        {
            extraText.alpha = 0;
        }

        for (int i = 0; i < milestoneObjects.Length; i++)
        {
            if (milestoneObjects[i] != null)
            {
                milestoneObjects[i].SetActive(i == index);
            }
        }

        isOpen = true;
        closeUnlockTime = Time.unscaledTime + closeUnlockDelay;

        // Panel ditampilkan lebih dulu; bagian lain dijaga null supaya tidak menghentikan tutorial
        if (transition != null)
        {
            transition.Show();
        }
        else
        {
            Debug.LogWarning("TutorialManager: 'transition' belum diisi di Inspector, panel tidak akan muncul.");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MuffleAudio(0.6f);
        }

        if (slowTimeCoroutine != null)
        {
            StopCoroutine(slowTimeCoroutine);
        }

        slowTimeCoroutine = StartCoroutine(SlowTime(index));

        // DIAGNOSTIK sementara: hapus baris ini setelah masalah beres
        Debug.Log("[Tutorial] ShowTutorial(" + index + ") panel='" + milestoneObjects[index].name +
                  "' activeSelf=" + milestoneObjects[index].activeSelf +
                  " activeInHierarchy=" + milestoneObjects[index].activeInHierarchy +
                  " transition=" + (transition != null ? transition.name : "NULL") +
                  " animator=" + (animators != null && animators[index] != null) +
                  " timeScale=" + Time.timeScale);

        // Animasi dijalankan terakhir & dibungkus try-catch: kalau ada masalah, tutorial tetap muncul
        PlayPanelAnimation(index);
    }

    private void PlayPanelAnimation(int index)
    {
        try
        {
            if (animators != null && animators[index] != null)
            {
                animators[index].Play(revealDelay);
            }

            if (alertCoroutine != null)
            {
                StopCoroutine(alertCoroutine);
                alertCoroutine = null;
            }

            if (extraText != null)
            {
                extraText.rectTransform.localRotation = extraRestRotation;
            }

            if (extraText != null && index > 0)
            {
                alertCoroutine = StartCoroutine(AlertSwing());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            if (extraText != null) extraText.alpha = 1f;
        }
    }

    // Tag "New Trash Type!!" muncul sambil berayun (UIPulse sudah mengatur skala, jadi yang dianimasikan rotasi)
    private IEnumerator AlertSwing()
    {
        RectTransform rect = extraText.rectTransform;
        Quaternion restRotation = extraRestRotation;

        extraText.alpha = 0f;
        rect.localRotation = restRotation * Quaternion.Euler(0f, 0f, 35f);

        float wait = alertDelay;
        while (wait > 0f)
        {
            wait -= Time.unscaledDeltaTime;
            yield return null;
        }

        float duration = 0.45f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / duration);

            // ease out back: sedikit melewati lalu kembali
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float m = p - 1f;
            float back = 1f + c3 * m * m * m + c1 * m * m;

            extraText.alpha = Mathf.Clamp01(p * 4f);
            rect.localRotation = restRotation * Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(35f, 0f, back));
            yield return null;
        }

        extraText.alpha = 1f;
        rect.localRotation = restRotation;
        alertCoroutine = null;
    }

    private IEnumerator SlowTime(int index)
    {
        if (index == 0)
        {
            Time.timeScale = 0f;
            yield break;
        }

        if (index > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        float elapsed = 0;
        while (elapsed < 1.0f)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1f, 0f, elapsed);
            yield return null;
        }
        Time.timeScale = 0f;
    }

    public void CloseTutorial()
    {
        isOpen = false;

        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
            alertCoroutine = null;
        }

        if (extraText != null)
        {
            extraText.rectTransform.localRotation = extraRestRotation;
        }

        if (slowTimeCoroutine != null)
        {
            StopCoroutine(slowTimeCoroutine);
            slowTimeCoroutine = null;
        }

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UnmuffleAudio(0.6f);
        }

        if (transition != null)
        {
            transition.Hide();
        }

        if (initialTutorial)
        {
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.StartTimer();
            }
            initialTutorial = false;
        }
    }
}