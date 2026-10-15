using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Custom cursor (hand). Created automatically at start.
//  - Open    : open hand (Default main cursor across the game)
//  - Grab    : closed fist (when holding/dragging trash or tape)
//  - Pointer : pointing finger (when hovering OR clicking UI buttons / opened boxes)
public class CustomCursor : MonoBehaviour
{
    private enum State { Pointer, Open, Grab }

    // Attributes

    private static CustomCursor instance;

    [SerializeField] private float heightRatio = 0.075f;
    [SerializeField] private float minHeight = 44f;
    [SerializeField] private float maxHeight = 110f;

    private Texture2D[] textures = new Texture2D[3];
    private readonly Vector2[] hotspots =
    {
        new Vector2(0.338f, 0.027f),   // Pointer: fingertip
        new Vector2(0.5f, 0.5f),       // Open: center palm
        new Vector2(0.5f, 0.5f)        // Grab: center fist
    };

    private RectTransform rect;
    private RawImage image;
    private State state = State.Open;
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
                Debug.LogWarning("CustomCursor: image '" + names[i] + "' not found in Assets/Resources/Cursors/. Default OS cursor used.");
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
        image.texture = textures[(int)State.Open];
        image.enabled = false;
    }

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

        // 1. Identify interactive targets
        bool isClickable = IsOverButton(pos) || IsOverFlattenableBox(pos);
        bool overDraggable = !isClickable && IsOverDraggable(pos);

        // 2. Grabbing is only triggered on draggable items (never on buttons or flattenable boxes)
        if (mouse.leftButton.wasPressedThisFrame)
        {
            grabbing = overDraggable;
        }
        if (!mouse.leftButton.isPressed)
        {
            grabbing = false;
        }

        // 3. Priority:
        //    - Clickable targets (Button or Flattenable Box) ALWAYS stay as Pointer (even when clicked!)
        //    - Active dragging stays as Grab
        //    - Everything else is Open
        State newState;
        if (isClickable)
        {
            newState = State.Pointer;
        }
        else if (grabbing)
        {
            newState = State.Grab;
        }
        else
        {
            newState = State.Open;
        }

        if (newState != state)
        {
            state = newState;
            image.texture = textures[(int)state];
            pop = 1.2f;
        }

        // Size & position
        Texture2D tex = textures[(int)state];
        float h = Mathf.Clamp(Screen.height * heightRatio, minHeight, maxHeight);
        float w = h * tex.width / tex.height;
        rect.sizeDelta = new Vector2(w, h);
        Vector2 hot = hotspots[(int)state];
        rect.pivot = new Vector2(hot.x, 1f - hot.y);
        rect.anchoredPosition = pos;

        // Scale feedback: Pointer presses down slightly when clicked, Open breathes, Grab squeezes
        float targetScale = 1f;
        if (state == State.Pointer && mouse.leftButton.isPressed) targetScale = 0.88f;
        else if (state == State.Open) targetScale = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.03f;
        else if (state == State.Grab) targetScale = 0.92f;

        scale = Mathf.Lerp(scale, targetScale, 1f - Mathf.Exp(-18f * dt));
        pop = Mathf.Lerp(pop, 1f, 1f - Mathf.Exp(-14f * dt));

        // Tilt when dragging
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

    private bool IsOverButton(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        uiResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiResults);

        if (uiResults.Count > 0)
        {
            Button btn = uiResults[0].gameObject.GetComponentInParent<Button>();
            if (btn != null && btn.interactable && btn.isActiveAndEnabled)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsOverFlattenableBox(Vector2 screenPos)
    {
        if (Time.timeScale <= 0.99f) return false;
        if (IsPointerOverUI(screenPos)) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider == null) return false;

        BoxLogic box = hit.collider.GetComponent<BoxLogic>();
        if (box == null) box = hit.collider.GetComponentInParent<BoxLogic>();

        return box != null && box.CanBeFlattened();
    }

    private bool IsOverDraggable(Vector2 screenPos)
    {
        if (Time.timeScale <= 0.99f) return false;
        if (IsPointerOverUI(screenPos)) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider == null) return false;

        // Check isActiveAndEnabled so disabled DragDrops on unflattened boxes are ignored
        DragDrop dd = hit.collider.GetComponent<DragDrop>();
        if (dd != null && dd.isActiveAndEnabled) return true;

        TapePuller tp = hit.collider.GetComponent<TapePuller>();
        if (tp != null && tp.isActiveAndEnabled) return true;

        ResidueTray rt = hit.collider.GetComponent<ResidueTray>();
        if (rt != null && rt.isActiveAndEnabled) return true;

        return false;
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        uiResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiResults);

        for (int i = 0; i < uiResults.Count; i++)
        {
            if (uiResults[i].gameObject.layer == 5) return true;
        }
        return false;
    }
}