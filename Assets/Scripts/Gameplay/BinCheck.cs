using System.Collections.Generic;
using UnityEngine;

public class BinCheck : MonoBehaviour
{
    // Attributes

    [SerializeField] private List<string> binTag = new List<string>();
    [SerializeField] private Transform destroyPoint;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float correctTime = 3f;

    [SerializeField] private AudioClip binSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;

    private string trashTag;
    private Vector3 originalScale;
    private bool isPulsing;


    // Method

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
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

    public void HandleTrashBin (GameObject trash)
    {
        if (GameTimer.Instance.timeRemaining <= 0) return;

        trashTag = trash.tag;
        TrashItem item = trash.GetComponent<TrashItem>();
        bool isCorrect = false;

        if (AudioManager.Instance != null && binSound != null)
        {
            AudioManager.Instance.PlaySFX(binSound);
        }


        foreach (string t in binTag)
        {
            if (trash.CompareTag(t))
            {
                isCorrect = true;
                break;
            }
        }

        if (isCorrect)
        {
            //Debug.Log("True");
            ScoreManager.instance.AddScore(item.GetScore());
            GameTimer.Instance.AddTimer(correctTime);

            if (AudioManager.Instance != null && correctSound != null)
            {
                AudioManager.Instance.PlaySFX(correctSound);
            }
        }
        else
        {
            //Debug.Log("False");
            ScoreManager.instance.SubtractScore();

            if (AudioManager.Instance != null && wrongSound != null)
            {
                AudioManager.Instance.PlaySFX(wrongSound);
            }
        }
    }

    public Vector3 GetDestroyPoint()
    {
        return destroyPoint.transform.position;
    }

    public void Pulse()
    {
        isPulsing = true;
    }
}
