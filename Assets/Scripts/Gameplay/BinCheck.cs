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

    // Polish: VFX bin
    [Header("VFX")]
    [SerializeField] private bool enableVFX = true;
    [SerializeField] private float vfxScale = 1f;            // besar semua efek (naikkan kalau terlalu kecil)
    [SerializeField] private int vfxSortingBoost = 20;       // naikkan kalau efek tertutup sprite bin

    private string trashTag;
    private Vector3 originalScale;
    private bool isPulsing;
    private SpriteRenderer binRenderer;


    // Method

    private void Awake()
    {
        originalScale = transform.localScale;
        binRenderer = GetComponent<SpriteRenderer>();
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
            // Skor dasar sampah ini dan multiplier yang dipakai SAAT INI.
            // Multiplier harus dibaca SEBELUM AddScore(), karena AddScore menaikkannya sesudah menghitung skor.
            int baseScore = item != null ? item.GetScore() : 0;
            int multUsed = Mathf.Max(1, ScoreManager.instance.GetMult());
            int shownScore = baseScore * multUsed;   // sama dengan hitungan di ScoreManager.AddScore

            ScoreManager.instance.AddScore(baseScore);
            GameTimer.Instance.AddTimer(correctTime);

            if (AudioManager.Instance != null && correctSound != null)
            {
                AudioManager.Instance.PlaySFX(correctSound);
            }

            // VFX hanya untuk jawaban benar. Dipanggil di frame yang sama dengan Pulse()
            // (DragDrop memanggil HandleTrashBin lalu Pulse), jadi keduanya tampil bersamaan.
            if (enableVFX)
            {
                SimpleVFX.Correct(GetVFXPosition(), shownScore, multUsed, vfxScale, GetVFXReference(trash), vfxSortingBoost);
            }
        }
        else
        {
            ScoreManager.instance.SubtractScore();

            if (AudioManager.Instance != null && wrongSound != null)
            {
                AudioManager.Instance.PlaySFX(wrongSound);
            }

            // Efek berbeda untuk bin yang salah: ring merah kecil saja
            if (enableVFX)
            {
                SimpleVFX.Wrong(GetVFXPosition(), vfxScale, GetVFXReference(trash), vfxSortingBoost);
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

    private Vector3 GetVFXPosition()
    {
        return destroyPoint != null ? destroyPoint.position : transform.position;
    }

    // Acuan sorting & material efek: sprite bin kalau ada, kalau tidak sprite sampah
    private SpriteRenderer GetVFXReference(GameObject trash)
    {
        if (binRenderer != null) return binRenderer;
        return trash != null ? trash.GetComponent<SpriteRenderer>() : null;
    }
}