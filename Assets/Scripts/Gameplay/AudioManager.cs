using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    // Attributes

    public static AudioManager Instance;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerSnapshot normalSnapshot;
    [SerializeField] private AudioMixerSnapshot muffledSnapshot;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private string volumeParameterName = "MasterVolume";

    private Coroutine fadeCoroutine;

    // Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnmuffleAudio(0.1f);
        FadeVolumeIn(0f);
    }

    public void MuffleAudio(float duration)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(0.15f, duration));
#else
        if (muffledSnapshot != null)
        {
            muffledSnapshot.TransitionTo(duration);
        }
#endif
    }

    public void UnmuffleAudio(float duration)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(1.0f, duration));
#else
        if (normalSnapshot != null)
        {
            normalSnapshot.TransitionTo(duration);
        }
#endif
    }

    public void FadeVolumeOut(float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(0.0001f, duration));
    }

    public void FadeVolumeIn(float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(1.0f, duration));
    }

    private IEnumerator FadeVolumeRoutine(float targetVolumeValue, float duration)
    {
        if (mixer == null) yield break;

        float startVal;
        mixer.GetFloat(volumeParameterName, out startVal);

        float startVolumeValue = Mathf.Pow(10f, startVal / 20f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float currentVolume = Mathf.Lerp(startVolumeValue, targetVolumeValue, t);

            float db = Mathf.Log10(Mathf.Max(currentVolume, 0.0001f)) * 20f;
            mixer.SetFloat(volumeParameterName, db);

            yield return null;
        }

        float finalDb = Mathf.Log10(Mathf.Max(targetVolumeValue, 0.0001f)) * 20f;
        mixer.SetFloat(volumeParameterName, finalDb);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
