using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Cursor kustom (tangan). Dibuat otomatis saat game mulai, jadi TIDAK perlu
// dipasang di scene mana pun. Gambar diambil dari Assets/Resources/Cursors/.
//
//  - Pointer : tangan menunjuk (default, juga di atas tombol UI)
//  - Open    : tangan terbuka, saat mouse di atas sampah yang bisa di-drag
//  - Grab    : tangan menggenggam, saat sampah sedang di-drag
public class CustomCursor : MonoBehaviour
{
    private enum State { Pointer, Open, Grab }

    // Attributes

    private static CustomCursor instance;

    [SerializeField] private float heightRatio = 0.075f;   // tinggi cursor = 7.5% tinggi layar
    [SerializeField] private float minHeight = 44f;
    [SerializeField] private float maxHeight = 110f;

    private Texture2D[] textures = new Texture2D[3];
    // Titik klik (hotspot) tiap cursor: x dari kiri, y dari ATAS, nilai 0-1.
    private readonly Vector2[] hotspots =
    {
        new Vector2(0.338f, 0.027f),   // Pointer: ujung jari telunjuk
        new Vector2(0.5f, 0.5f),       // Open: tengah telapak
        new Vector2(0.5f, 0.5f)        // Grab: tengah genggaman
    };

    private RectTransform rect;
    private RawImage image;
    private State state = State.Pointer;
    private bool grabbing;
    private bool hasFocus = true;

    private float scale = 1f;
    private float pop = 1f;
    private float rotation;
    private Vector2 lastPos;

    private readonly List<RaycastResult> uiResults = new List<RaycastResult>();

    // Methods

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;

        GameObject go = new GameObject("Custom Cursor");
        DontDestroyOnLoad(go);
        go.AddComponent<CustomCursor>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        textures[(int)State.Pointer] = LoadCursor("cursor_pointer");
        textures[(int)State.Open] = LoadCursor("cursor_open");
        textures[(int)State.Grab] = LoadCursor("cursor_grab");

        for (int i = 0; i < textures.Length; i++)
        {
            if (textures[i] == null)
            {
                string[] names = { "cursor_pointer", "cursor_open", "cursor_grab" };
                Debug.LogWarning("CustomCursor: gambar '" + names[i] + "' tidak ditemukan. Pastikan ada file Assets/Resources/Cursors/" + names[i] + ".png. Cursor bawaan dipakai.");
                instance = null;
                Destroy(gameObject);
                return;
            }
        }

        GameObject canvasObj = new GameObject("Cursor Canvas");
        canvasObj.transform.SetParent(transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        GameObject imageObj = new GameObject("Cursor Image", typeof(RectTransform), typeof(RawImage));
        imageObj.transform.SetParent(canvasObj.transform, false);
        rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        image = imageObj.GetComponent<RawImage>();
        image.raycastTarget = false;
        image.texture = textures[(int)State.Pointer];
        image.enabled = false;
    }

    // Bisa membaca gambar yang diimpor sebagai Texture maupun Sprite
    private static Texture2D LoadCursor(string fileName)
    {
        Texture2D tex = Resources.Load<Texture2D>("Cursors/" + fileName);
        if (tex != null) return tex;

        Sprite sprite = Resources.Load<Sprite>("Cursors/" + fileName);
        if (sprite != null) return sprite.texture;

        return null;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            Cursor.visible = true;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        hasFocus = focus;
    }

    private void Update()
    {
        if (image == null) return;

        Mouse mouse = Mouse.current;

        // Di HP / tablet tidak ada cursor, dan kalau tidak ada mouse pakai cursor bawaan.
        bool touchDevice = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
        if (touchDevice || mouse == null || !hasFocus)
        {
            image.enabled = false;
            Cursor.visible = true;
            return;
        }

        Vector2 pos = mouse.position.ReadValue();
        bool inside = pos.x >= 0f && pos.y >= 0f && pos.x <= Screen.width && pos.y <= Screen.height;
        image.enabled = inside;
        Cursor.visible = !inside;
        if (!inside) return;

        float dt = Time.unscaledDeltaTime;

        // Tentukan state cursor
        bool overDraggable = IsOverDraggable(pos);

        if (mouse.leftButton.wasPressedThisFrame)
        {
            grabbing = overDraggable;
        }
        if (!mouse.leftButton.isPressed)
        {
            grabbing = false;
        }

        State newState = grabbing ? State.Grab : (overDraggable ? State.Open : State.Pointer);
        if (newState != state)
        {
            state = newState;
            image.texture = textures[(int)state];
            pop = 1.2f;   // efek "pop" kecil saat ganti bentuk
        }

        // Ukuran & posisi (pivot = hotspot, jadi ujung jari tepat di posisi mouse)
        Texture2D tex = textures[(int)state];
        float h = Mathf.Clamp(Screen.height * heightRatio, minHeight, maxHeight);
        float w = h * tex.width / tex.height;
        rect.sizeDelta = new Vector2(w, h);
        Vector2 hot = hotspots[(int)state];
        rect.pivot = new Vector2(hot.x, 1f - hot.y);
        rect.anchoredPosition = pos;

        // Animasi skala: menekan = mengecil sedikit, Open = bernapas pelan
        float targetScale = 1f;
        if (state == State.Pointer && mouse.leftButton.isPressed) targetScale = 0.88f;
        else if (state == State.Open) targetScale = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.03f;
        else if (state == State.Grab) targetScale = 0.92f;

        scale = Mathf.Lerp(scale, targetScale, 1f - Mathf.Exp(-18f * dt));
        pop = Mathf.Lerp(pop, 1f, 1f - Mathf.Exp(-14f * dt));

        // Miring sedikit mengikuti arah gerak mouse saat menggenggam
        float targetRot = 0f;
        if (state == State.Grab && dt > 0f)
        {
            float velX = (pos.x - lastPos.x) / dt;
            targetRot = Mathf.Clamp(-velX * 0.01f, -15f, 15f);
        }
        rotation = Mathf.Lerp(rotation, targetRot, 1f - Mathf.Exp(-12f * dt));
        lastPos = pos;

        rect.localScale = Vector3.one * (scale * pop);
        rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    // Sama seperti StartDrag() di DragDrop.cs: ada sampah di bawah mouse & tidak tertutup UI.
    private bool IsOverDraggable(Vector2 screenPos)
    {
        if (Time.timeScale <= 0.99f) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider == null || hit.collider.GetComponent<DragDrop>() == null) return false;

        return !IsPointerOverUI(screenPos);
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPos;
        uiResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiResults);

        for (int i = 0; i < uiResults.Count; i++)
        {
            if (uiResults[i].gameObject.layer == 5) return true;
        }
        return false;
    }
}