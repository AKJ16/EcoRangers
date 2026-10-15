using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Menganimasikan isi satu panel tutorial (Milestone 0-5) secara otomatis.
// Script ini ditambahkan oleh TutorialManager lewat kode, jadi TIDAK perlu
// dipasang manual di scene. Urutan kemunculan:
//   1. Judul (pop)  ->  2. Bin (pop)  ->  3. Sampah (pop)
//   4. Panah (geser masuk)  ->  5. Teks penjelasan (fade + naik)
// Setelah muncul, panah bergerak maju-mundur dan sampah naik-turun pelan.
public class TutorialPanelAnimator : MonoBehaviour
{
    private enum Kind { Title, Bin, Item, Arrow, Text }

    private class Element
    {
        public RectTransform rect;
        public CanvasGroup group;
        public Kind kind;
        public bool hasPulse;          // sudah dianimasikan UIPulse, jangan ubah skalanya
        public Vector3 restScale;
        public Vector2 restPos;
        public Vector2 direction;      // arah panah (dalam ruang parent)
        public float delay;
        public float duration;
        public float phase;
    }

    // Attributes

    [SerializeField] private float binStagger = 0.10f;
    [SerializeField] private float itemStagger = 0.07f;
    [SerializeField] private float arrowStagger = 0.06f;
    [SerializeField] private float textStagger = 0.10f;
    [SerializeField] private float arrowNudge = 8f;      // jarak gerak panah (pixel)
    [SerializeField] private float itemBob = 3f;         // jarak naik-turun sampah (pixel)

    private readonly List<Element> elements = new List<Element>();
    private bool collected;
    private Coroutine routine;

    // Methods

    public void Play(float startDelay)
    {
        if (!collected)
        {
            Collect();
        }

        if (routine != null)
        {
            StopCoroutine(routine);
        }

        // Sembunyikan semua dulu sebelum panel turun ke layar
        for (int i = 0; i < elements.Count; i++)
        {
            Apply(elements[i], -1f);
        }

        routine = StartCoroutine(Run(startDelay));
    }

    private void OnDisable()
    {
        routine = null;
        // Kembalikan semua ke kondisi awal supaya tidak ada sisa animasi
        for (int i = 0; i < elements.Count; i++)
        {
            Element e = elements[i];
            if (e.rect == null) continue;
            e.rect.anchoredPosition = e.restPos;
            if (!e.hasPulse) e.rect.localScale = e.restScale;
            e.group.alpha = 1f;
        }
    }

    private IEnumerator Run(float startDelay)
    {
        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime;
            float local = t - startDelay;

            for (int i = 0; i < elements.Count; i++)
            {
                Apply(elements[i], local);
            }
            yield return null;
        }
    }

    private void Collect()
    {
        collected = true;

        List<Element> titles = new List<Element>();
        List<Element> bins = new List<Element>();
        List<Element> items = new List<Element>();
        List<Element> arrows = new List<Element>();
        List<Element> texts = new List<Element>();

        RectTransform[] all = GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            RectTransform rt = all[i];
            if (rt == transform) continue;

            // Wadah kosong (grup) dilewati, yang dianimasikan adalah isinya
            Graphic graphic = rt.GetComponent<Graphic>();
            if (graphic == null) continue;

            Element e = new Element();
            e.rect = rt;
            e.restScale = rt.localScale;
            e.restPos = rt.anchoredPosition;
            e.hasPulse = rt.GetComponent<UIPulse>() != null;
            e.phase = Random.value * Mathf.PI * 2f;

            if (!rt.TryGetComponent(out CanvasGroup group))
            {
                group = rt.gameObject.AddComponent<CanvasGroup>();
            }
            e.group = group;

            string n = rt.name.ToLowerInvariant();

            if (graphic is TMP_Text)
            {
                if (n.StartsWith("text tutorial"))
                {
                    e.kind = Kind.Title;
                    titles.Add(e);
                }
                else
                {
                    e.kind = Kind.Text;
                    texts.Add(e);
                }
            }
            else if (n.StartsWith("arrow"))
            {
                e.kind = Kind.Arrow;
                // Gambar panah menunjuk ke ATAS saat rotasi 0, jadi arahnya = "atas" lokal
                Vector3 up = rt.localRotation * Vector3.up;
                e.direction = new Vector2(up.x, up.y).normalized;
                arrows.Add(e);
            }
            else if (n.StartsWith("bin") || n.StartsWith("clue") || n.Contains("tray"))
            {
                e.kind = Kind.Bin;
                bins.Add(e);
            }
            else
            {
                e.kind = Kind.Item;
                items.Add(e);
            }
        }

        // Susun jadwal kemunculan: tiap kelompok mulai setelah kelompok sebelumnya
        float cursor = 0f;
        cursor = Schedule(titles, cursor, 0.10f, 0.40f);
        cursor = Schedule(bins, cursor, binStagger, 0.35f);
        cursor = Schedule(items, cursor + 0.05f, itemStagger, 0.35f);
        cursor = Schedule(arrows, cursor + 0.05f, arrowStagger, 0.30f);
        Schedule(texts, cursor + 0.05f, textStagger, 0.40f);
    }

    private float Schedule(List<Element> list, float start, float stagger, float duration)
    {
        float t = start;
        for (int i = 0; i < list.Count; i++)
        {
            list[i].delay = t;
            list[i].duration = duration;
            elements.Add(list[i]);
            t += stagger;
        }
        return t;
    }

    // local < 0 artinya belum mulai (disembunyikan)
    private void Apply(Element e, float local)
    {
        if (e.rect == null) return;

        float p = Mathf.Clamp01((local - e.delay) / e.duration);
        bool started = local >= e.delay;

        if (!started)
        {
            e.group.alpha = 0f;
            return;
        }

        float eased = EaseOutCubic(p);
        float back = EaseOutBack(p);
        Vector2 pos = e.restPos;
        float time = Time.unscaledTime;

        switch (e.kind)
        {
            case Kind.Title:
                e.group.alpha = Mathf.Clamp01(p * 3f);
                if (!e.hasPulse) e.rect.localScale = e.restScale * Mathf.LerpUnclamped(0.6f, 1f, back);
                break;

            case Kind.Bin:
                e.group.alpha = Mathf.Clamp01(p * 3f);
                if (!e.hasPulse) e.rect.localScale = e.restScale * back;
                break;

            case Kind.Item:
                e.group.alpha = Mathf.Clamp01(p * 3f);
                if (!e.hasPulse) e.rect.localScale = e.restScale * back;
                if (p >= 1f)
                {
                    pos.y += Mathf.Sin(time * 2.2f + e.phase) * itemBob;
                }
                break;

            case Kind.Arrow:
                e.group.alpha = eased;
                pos -= e.direction * (20f * (1f - eased));   // geser masuk dari belakang
                if (p >= 1f)
                {
                    float nudge = (Mathf.Sin(time * 5f + e.phase) * 0.5f + 0.5f) * arrowNudge;
                    pos += e.direction * nudge;
                }
                break;

            case Kind.Text:
                e.group.alpha = eased;
                pos.y -= 14f * (1f - eased);                 // naik pelan ke posisi akhir
                break;
        }

        e.rect.anchoredPosition = pos;
    }

    private static float EaseOutCubic(float x)
    {
        float inv = 1f - x;
        return 1f - inv * inv * inv;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float m = x - 1f;
        return 1f + c3 * m * m * m + c1 * m * m;
    }
}