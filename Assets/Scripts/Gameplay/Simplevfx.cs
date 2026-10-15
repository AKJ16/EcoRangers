using System.Collections.Generic;
using TMPro;
using UnityEngine;

// VFX sederhana yang dibuat 100% lewat kode (tanpa prefab, material, atau aset).
//   SimpleVFX.PickUp(spriteRenderer)                 -> ring kecil + percikan saat sampah diambil
//   SimpleVFX.Correct(pos, skor, mult, ...)          -> ring hijau + sparkle + teks "+skor xMult" (outline hitam)
//   SimpleVFX.Wrong(pos, ...)                        -> ring merah kecil
// Objek VFX menghancurkan dirinya sendiri setelah selesai dan memakai waktu
// tanpa timeScale, jadi tetap selesai walau game sedang diperlambat.
public class SimpleVFX : MonoBehaviour
{
    private enum Mode { Ring, Burst, Text }

    private class Part
    {
        public Transform tf;
        public SpriteRenderer sr;
        public Color baseColor;
        public Vector3 velocity;
        public float spin;
        public float size;
    }

    // Sprite dibuat sekali lewat kode lalu dipakai ulang
    private static Sprite circleSprite;
    private static Sprite ringSprite;
    private static Sprite starSprite;

    private Mode mode;
    private float duration = 1f;
    private float elapsed;
    private Color color = Color.white;
    private float startSize = 1f;
    private float endSize = 1f;
    private float riseHeight;
    private Vector3 startPos;
    private SpriteRenderer ringRenderer;
    private TextMeshPro label;
    private readonly List<TextMeshPro> outlines = new List<TextMeshPro>();
    private readonly List<Part> parts = new List<Part>();

    // Public API

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        circleSprite = null;
        ringSprite = null;
        starSprite = null;
    }

    // Saat sampah diambil dari conveyor / tray
    public static void PickUp(SpriteRenderer reference)
    {
        if (reference == null) return;

        float size = Mathf.Clamp(Mathf.Max(reference.bounds.size.x, reference.bounds.size.y), 0.4f, 2f);
        Vector3 pos = reference.bounds.center;
        pos.z = reference.transform.position.z;

        SpawnRing(pos, new Color(1f, 1f, 1f, 0.9f), size * 0.6f, size * 1.6f, 0.30f, reference, 10);
        SpawnBurst(pos, new Color(1f, 1f, 1f, 1f), new Color(0.8f, 1f, 0.85f, 1f), 6, size * 1.8f, size * 0.14f, 0.35f, reference, 10);
    }

    // Saat sampah masuk bin yang benar.
    // score = skor AKHIR (sudah dikali multiplier), multiplier = pengali yang dipakai.
    public static void Correct(Vector3 pos, int score, int multiplier, float scale, SpriteRenderer reference, int boost)
    {
        SpawnRing(pos, new Color(0.55f, 1f, 0.5f, 0.95f), 0.4f * scale, 2.0f * scale, 0.5f, reference, boost);
        SpawnBurst(pos, new Color(1f, 0.92f, 0.35f, 1f), new Color(0.6f, 1f, 0.55f, 1f), 14, 3.2f * scale, 0.28f * scale, 0.6f, reference, boost + 1);

        // Contoh: "+30 x3". Tanpa multiplier hanya "+10".
        string text = "+" + score;
        if (multiplier > 1)
        {
            text += " <size=65%>x" + multiplier + "</size>";
        }

        // Makin besar multiplier, teks sedikit makin besar (maks sekitar +32%)
        float sizeBoost = 1f + 0.08f * Mathf.Clamp(multiplier - 1, 0, 4);

        SpawnText(pos + Vector3.up * 0.5f * scale, text, new Color(1f, 0.95f, 0.4f, 1f), 5f * scale * sizeBoost, reference, boost + 5);
    }

    // Saat sampah masuk bin yang salah
    public static void Wrong(Vector3 pos, float scale, SpriteRenderer reference, int boost)
    {
        SpawnRing(pos, new Color(1f, 0.3f, 0.25f, 0.9f), 0.4f * scale, 1.5f * scale, 0.4f, reference, boost);
    }

    // Tray Addition

    // Simple white pulse when placing regular trash on the tray
    public static void TraySimplePulse(Vector3 pos, float scale = 1f, SpriteRenderer reference = null, int boost = 20)
    {
        SpawnRing(pos, new Color(1f, 1f, 1f, 0.95f), 0.4f * scale, 2.0f * scale, 0.45f, reference, boost);
    }

    // White VFX (Ring + Sparkle Burst) for complex trash (B3 or Residue)
    public static void TrayComplex(Vector3 pos, float scale = 1f, SpriteRenderer reference = null, int boost = 20)
    {
        SpawnRing(pos, new Color(1f, 1f, 1f, 0.95f), 0.5f * scale, 2.5f * scale, 0.5f, reference, boost);
        SpawnBurst(pos, new Color(1f, 1f, 1f, 1f), new Color(0.9f, 0.9f, 0.95f, 1f), 14, 3.2f * scale, 0.28f * scale, 0.6f, reference, boost + 1);
    }

    // Yellow-orange VFX (Ring + Sparkle Burst) for Composite trash
    public static void TrayComposite(Vector3 pos, float scale = 0.6f, SpriteRenderer reference = null, int boost = 20)
    {
        SpawnRing(pos, new Color(1f, 0.95f, 0.2f, 0.95f), 0.6f * scale, 3.0f * scale, 0.5f, reference, boost);
        SpawnBurst(pos, new Color(1f, 1f, 0.3f, 1f), new Color(1f, 0.85f, 0.1f, 1f), 14, 3.2f * scale, 0.28f * scale, 0.6f, reference, boost + 1);
    }

    // Red grab-style burst for "UpcBottle" on the Processing Tray
    public static void GrabPulseRed(Vector3 pos, float scale = 0.5f, SpriteRenderer reference = null, int boost = 20)
    {
        SpawnRing(pos, new Color(0.25f, 0f, 0f, 0.95f), 0.4f * scale, 2.0f * scale, 0.45f, reference, boost);
        SpawnBurst(pos, new Color(0.35f, 0f, 0f, 1f), new Color(0.55f, 0f, 0f, 1f), 10, 3.0f * scale, 0.26f * scale, 0.5f, reference, boost + 1);
    }

    // Yellow grab-style burst for Box Unseal & Flatten (same yellow as composite trash)
    public static void GrabPulseYellow(Vector3 pos, float scale = 0.5f, SpriteRenderer reference = null, int boost = 20)
    {
        SpawnRing(pos, new Color(1f, 0.95f, 0.2f, 0.95f), 0.5f * scale, 2.5f * scale, 0.45f, reference, boost);
        SpawnBurst(pos, new Color(1f, 1f, 0.3f, 1f), new Color(1f, 0.85f, 0.1f, 1f), 10, 3.0f * scale, 0.26f * scale, 0.5f, reference, boost + 1);
    }

    // Pop effect when a new bin emerges from the screen edge into the scene
    public static void BinPop(Vector3 pos, float scale = 1.2f, SpriteRenderer reference = null, int boost = 25)
    {
        Color ringColor = new Color(1f, 0.92f, 0.4f, 0.95f); // Bright golden-white
        Color sparkA = Color.white;
        Color sparkB = new Color(1f, 0.82f, 0.25f, 1f);

        SpawnRing(pos, ringColor, 0.4f * scale, 2.2f * scale, 0.5f, reference, boost);
        SpawnBurst(pos, sparkA, sparkB, 14, 3.5f * scale, 0.28f * scale, 0.55f, reference, boost + 1);
    }

    // Animasi

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        float p = Mathf.Clamp01(elapsed / duration);

        switch (mode)
        {
            case Mode.Ring:
            {
                if (ringRenderer != null)
                {
                    float size = Mathf.Lerp(startSize, endSize, EaseOutCubic(p));
                    transform.localScale = Vector3.one * size;
                    Color c = color;
                    c.a = color.a * (1f - p * p);
                    ringRenderer.color = c;
                }
                break;
            }

            case Mode.Burst:
            {
                float dt = Time.unscaledDeltaTime;
                float drag = Mathf.Exp(-3.5f * dt);
                float fade = 1f - p * p;

                for (int i = 0; i < parts.Count; i++)
                {
                    Part part = parts[i];
                    if (part.tf == null) continue;

                    part.velocity *= drag;
                    part.tf.position += part.velocity * dt;
                    part.tf.Rotate(0f, 0f, part.spin * dt);
                    part.tf.localScale = Vector3.one * (part.size * fade);

                    Color c = part.baseColor;
                    c.a = part.baseColor.a * fade;
                    part.sr.color = c;
                }
                break;
            }

            case Mode.Text:
            {
                if (label == null) break;

                transform.position = startPos + Vector3.up * (EaseOutCubic(p) * riseHeight);
                float pop = p < 0.2f ? EaseOutBack(p / 0.2f) : 1f;
                transform.localScale = Vector3.one * Mathf.Max(0.01f, pop);

                float alpha = p < 0.6f ? 1f : 1f - (p - 0.6f) / 0.4f;
                label.alpha = alpha;
                for (int i = 0; i < outlines.Count; i++)
                {
                    if (outlines[i] != null) outlines[i].alpha = alpha;
                }
                break;
            }
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }

    // Pembuat VFX

    private static void SpawnRing(Vector3 pos, Color color, float from, float to, float duration, SpriteRenderer reference, int boost)
    {
        GameObject go = new GameObject("VFX Ring");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * from;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetRing();
        sr.color = color;
        ApplySorting(sr, reference, boost);

        SimpleVFX vfx = go.AddComponent<SimpleVFX>();
        vfx.mode = Mode.Ring;
        vfx.duration = duration;
        vfx.color = color;
        vfx.startSize = from;
        vfx.endSize = to;
        vfx.ringRenderer = sr;
    }

    private static void SpawnBurst(Vector3 pos, Color colorA, Color colorB, int count, float speed, float size, float duration, SpriteRenderer reference, int boost)
    {
        GameObject go = new GameObject("VFX Burst");
        go.transform.position = pos;

        SimpleVFX vfx = go.AddComponent<SimpleVFX>();
        vfx.mode = Mode.Burst;
        vfx.duration = duration;

        for (int i = 0; i < count; i++)
        {
            GameObject child = new GameObject("Spark");
            child.transform.SetParent(go.transform, false);

            SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
            sr.sprite = (i % 2 == 0) ? GetStar() : GetCircle();
            Color baseColor = Color.Lerp(colorA, colorB, Random.value);
            sr.color = baseColor;
            ApplySorting(sr, reference, boost);

            // Arah merata ke segala sisi, dengan sedikit acak
            float angle = (i + Random.value * 0.6f) / count * Mathf.PI * 2f;
            float spd = speed * Random.Range(0.6f, 1f);

            Part part = new Part();
            part.tf = child.transform;
            part.sr = sr;
            part.baseColor = baseColor;
            part.velocity = new Vector3(Mathf.Cos(angle) * spd, Mathf.Sin(angle) * spd, 0f);
            part.spin = Random.Range(-240f, 240f);
            part.size = size * Random.Range(0.7f, 1.3f);
            child.transform.localScale = Vector3.one * part.size;
            vfx.parts.Add(part);
        }
    }

    private static void SpawnText(Vector3 pos, string text, Color color, float fontSize, SpriteRenderer reference, int boost)
    {
        GameObject go = new GameObject("VFX Text");
        go.transform.position = pos;

        SimpleVFX vfx = go.AddComponent<SimpleVFX>();

        int baseOrder = (reference != null) ? reference.sortingOrder + boost : 100 + boost;

        // Teks utama (kuning)
        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        SetupText(tmp, text, color, fontSize, reference, baseOrder);

        // Outline hitam: 8 salinan teks hitam di belakang, digeser ke 8 arah.
        // Tidak bergantung pada shader/font, jadi pasti terlihat.
        // Kalau outline terlalu tipis/tebal, ubah 0.009f (mis. 0.012f = lebih tebal).
        float offset = fontSize * 0.009f;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 0.25f;

            GameObject child = new GameObject("Outline");
            child.transform.SetParent(go.transform, false);
            child.transform.localPosition = new Vector3(Mathf.Cos(angle) * offset, Mathf.Sin(angle) * offset, 0f);

            TextMeshPro outline = child.AddComponent<TextMeshPro>();
            SetupText(outline, text, Color.black, fontSize, reference, baseOrder - 1);
            vfx.outlines.Add(outline);
        }

        vfx.mode = Mode.Text;
        vfx.duration = 0.9f;
        vfx.riseHeight = 0.8f;
        vfx.startPos = pos;
        vfx.label = tmp;
    }

    private static void SetupText(TextMeshPro tmp, string text, Color color, float fontSize, SpriteRenderer reference, int order)
    {
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;

        if (reference != null)
        {
            tmp.sortingLayerID = reference.sortingLayerID;
        }
        tmp.sortingOrder = order;
    }

    private static void ApplySorting(SpriteRenderer target, SpriteRenderer reference, int boost)
    {
        if (reference != null)
        {
            if (reference.sharedMaterial != null)
            {
                target.sharedMaterial = reference.sharedMaterial;
            }
            target.sortingLayerID = reference.sortingLayerID;
            target.sortingOrder = reference.sortingOrder + boost;
        }
        else
        {
            target.sortingOrder = 100 + boost;
        }
    }

    // Sprite buatan kode (tekstur 64x64 putih, hanya alpha yang berbeda)

    private static Sprite GetCircle()
    {
        if (circleSprite == null)
        {
            circleSprite = MakeSprite((nx, ny) =>
            {
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                return (1f - d) * 8f;
            });
        }
        return circleSprite;
    }

    private static Sprite GetRing()
    {
        if (ringSprite == null)
        {
            ringSprite = MakeSprite((nx, ny) =>
            {
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                return Mathf.Clamp01((1f - d) * 10f) * Mathf.Clamp01((d - 0.82f) * 12f);
            });
        }
        return ringSprite;
    }

    private static Sprite GetStar()
    {
        if (starSprite == null)
        {
            starSprite = MakeSprite((nx, ny) =>
            {
                float v = Mathf.Pow(Mathf.Abs(nx), 0.5f) + Mathf.Pow(Mathf.Abs(ny), 0.5f);
                return (1f - v) * 3f;
            });
        }
        return starSprite;
    }

    private static Sprite MakeSprite(System.Func<float, float, float> alphaAt)
    {
        const int size = 64;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = ((x + 0.5f) / size) * 2f - 1f;
                float ny = ((y + 0.5f) / size) * 2f - 1f;
                byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alphaAt(nx, ny)) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();

        // 64 pixel per unit -> lebar 1 unit dunia pada skala 1
        return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect);
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