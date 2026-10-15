using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour
{

    // Attributes

    private Camera cam;
    private bool isDragging;
    private Vector3 offset;
    private Vector3 initialPos;
    private float camZDepth;
    private int trayLayer;
    private int binLayer;
    private TrayCheck tray;
    private ProcessingTray processTray;
    private bool onTray;
    private bool onProcessTray;
    private bool isReturning;
    private bool isDestroyed;
    private Vector3 binPos;

    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private float snapDistance = 1.0f;
    [SerializeField] private float destroySpeed = 15f;

    [SerializeField] private AudioClip dragSound;

    // Polish (semua bisa diatur di Inspector, nilai bawaan sudah cukup)
    [Header("Drag Polish")]
    [SerializeField] private float liftScale = 1.15f;        // sampah membesar saat diangkat
    [SerializeField] private float liftDuration = 0.14f;     // lama animasi mengangkat
    [SerializeField] private float swayPerSpeed = 2f;        // derajat miring per (unit/detik) gerak mouse
    [SerializeField] private float maxSway = 15f;            // miring maksimal (derajat)
    [SerializeField] private float idleWobble = 2.5f;        // goyang pelan saat diam (derajat)
    [SerializeField] private Vector2 shadowOffset = new Vector2(0.12f, -0.16f);
    [SerializeField, Range(0f, 1f)] private float shadowAlpha = 0.35f;
    [SerializeField] private float returnMinTime = 0.12f;    // durasi kembali ke posisi awal (min)
    [SerializeField] private float returnMaxTime = 0.45f;    // durasi kembali ke posisi awal (maks)
    [SerializeField] private bool pickUpEffect = true;       // VFX saat sampah diambil

    private SpriteRenderer spriteRenderer;
    private Transform shadow;
    private SpriteRenderer shadowRenderer;
    private Vector3 baseScale = Vector3.one;
    private Quaternion baseRotation = Quaternion.identity;
    private float liftT;
    private float swayAngle;
    private float lastDragX;
    private Vector3 returnStart;
    private float returnElapsed;
    private float returnDuration = 0.2f;

    // Methods

    void Awake()
    {
        cam = Camera.main;
        camZDepth = cam.WorldToScreenPoint(transform.position).z;
        trayLayer = LayerMask.NameToLayer("Tray");
        binLayer = LayerMask.NameToLayer("Bin");
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Time.timeScale <= 0.99f)
        {
            if (isDragging)
            {
                TrashItem item = gameObject.GetComponent<TrashItem>();
                if (item != null)
                {
                    item.StopMoving();
                }
            }
            return;
        }

        if (isReturning)
        {
            // Kembali ke posisi awal dengan ease (smoothstep): pelan di awal, pelan saat mendarat
            returnElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(returnElapsed / returnDuration);
            float eased = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(returnStart, initialPos, eased);

            if (t >= 1f)
            {
                transform.position = initialPos;
                isReturning = false;
            }

            return;
        }

        if (isDestroyed)
        {
            transform.position = Vector3.MoveTowards(transform.position, binPos, destroySpeed * 10 * Time.deltaTime);
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, destroySpeed * Time.deltaTime);

            if (transform.localScale.magnitude < 0.05f || Vector3.Distance(transform.position, binPos) < 0.01f)
            {
                transform.position = binPos;
                Destroy(gameObject);
            }

            return;
        }

        if (IsPrimaryClickedThisFrame())
        {
            StartDrag();
        }

        if (IsPrimaryClickHeld() && isDragging)
        {
            Drag();
        }

        if (IsPrimaryClickReleased())
        {
            if (isDragging)
            {
                // Skala & rotasi dikembalikan DULU sebelum CheckDrop(), supaya tray/bin
                // (mis. ProcessSprite) bebas mengatur skala & rotasi sampah sesudahnya.
                EndLift();
                CheckDrop();
            }
            isDragging = false;
        }
    }

    void StartDrag()
    {

        if (IsPointerOverUI()) return;

        Vector2 pointerPos = GetPointerPosition();
        Ray ray = cam.ScreenPointToRay(pointerPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            initialPos = transform.position;
            offset = transform.position - GetMouseWorldPosition(pointerPos);

            // Polish: simpan kondisi awal untuk efek angkat
            baseScale = transform.localScale;
            baseRotation = transform.rotation;
            liftT = 0f;
            swayAngle = 0f;
            lastDragX = transform.position.x;
            EnsureShadow();

            if (pickUpEffect)
            {
                SimpleVFX.PickUp(spriteRenderer);
            }

            if (AudioManager.Instance != null && dragSound != null)
            {
                AudioManager.Instance.PlaySFX(dragSound);
            }
        }
    }

    void Drag()
    {
        Vector2 pointerPos = GetPointerPosition();
        Vector3 trashPos = GetMouseWorldPosition(pointerPos) + offset;
        trashPos.z -= 2;
        transform.position = trashPos;

        UpdateLift(trashPos);
    }

    // Sampah membesar (pop kecil), miring mengikuti gerak mouse, dan bayangan mengikuti
    private void UpdateLift(Vector3 pos)
    {
        float dt = Time.deltaTime;

        liftT = Mathf.MoveTowards(liftT, 1f, dt / Mathf.Max(liftDuration, 0.01f));
        transform.localScale = baseScale * Mathf.LerpUnclamped(1f, liftScale, EaseOutBack(liftT));

        float velX = dt > 0f ? (pos.x - lastDragX) / dt : 0f;
        lastDragX = pos.x;

        float target = Mathf.Clamp(velX * swayPerSpeed, -maxSway, maxSway)
                       + Mathf.Sin(Time.time * 7f) * idleWobble * liftT;
        swayAngle = Mathf.Lerp(swayAngle, target, 1f - Mathf.Exp(-12f * dt));
        transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, swayAngle);

        if (shadow != null)
        {
            Vector3 o = new Vector3(shadowOffset.x, shadowOffset.y, 0f) * liftT;
            o.z = 0.05f;
            shadow.position = transform.position + o;
        }
    }

    private void EndLift()
    {
        transform.localScale = baseScale;
        transform.rotation = baseRotation;
        liftT = 0f;
        swayAngle = 0f;

        if (shadow != null)
        {
            shadow.gameObject.SetActive(false);
        }
    }

    // Bayangan = salinan sprite berwarna hitam transparan, dibuat sekali lalu dipakai ulang
    private void EnsureShadow()
    {
        if (spriteRenderer == null) return;

        if (shadow == null)
        {
            GameObject go = new GameObject("Drag Shadow");
            go.transform.SetParent(transform, false);
            shadow = go.transform;
            shadowRenderer = go.AddComponent<SpriteRenderer>();
        }

        // Sprite bisa berubah (ProcessSprite), jadi disegarkan tiap kali diambil
        shadowRenderer.sprite = spriteRenderer.sprite;
        if (spriteRenderer.sharedMaterial != null)
        {
            shadowRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
        }
        shadowRenderer.flipX = spriteRenderer.flipX;
        shadowRenderer.flipY = spriteRenderer.flipY;
        shadowRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        shadowRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        shadowRenderer.color = new Color(0f, 0f, 0f, shadowAlpha);

        shadow.localScale = Vector3.one;
        shadow.gameObject.SetActive(true);
    }

    Vector3 GetMouseWorldPosition(Vector2 screenPos)
    {
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camZDepth));
    }

    void CheckDrop()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, snapDistance);

        foreach (var hit in hits)
        {
            if (hit.gameObject.layer == trayLayer)
            {
                tray = hit.GetComponent<TrayCheck>();
                processTray = hit.GetComponent<ProcessingTray>();

                if (tray != null && tray.HandleTrashDrop(gameObject))
                {
                    onTray = true;
                    tray.Pulse();
                    return;
                }

                TrashItem item = GetComponent<TrashItem>();

                if (processTray != null && processTray.HandleTrashDrop(gameObject, item))
                {
                    onTray = true;
                    onProcessTray = true;
                    processTray.Pulse();
                    return;
                }
            }
            else if (hit.gameObject.layer == binLayer)
            {
                BinCheck bin = hit.GetComponent<BinCheck>();

                if (bin != null)
                {
                    //Debug.Log("Bin");
                    bin.HandleTrashBin(gameObject);
                    bin.Pulse();
                    binPos = bin.GetDestroyPoint();
                    isDestroyed = true;
                    if (onTray && tray != null)
                    {
                        tray.FreeTray();
                        onTray = false;
                    }
                    if (onProcessTray && processTray != null)
                    {
                        processTray.FreeTray();
                        onProcessTray = false;
                    }
                    return;
                }
            }
        }
        BeginReturn();
    }

    private void BeginReturn()
    {
        returnStart = transform.position;
        float distance = Vector3.Distance(returnStart, initialPos);
        returnDuration = Mathf.Clamp(distance / Mathf.Max(returnSpeed, 0.01f), returnMinTime, returnMaxTime);
        returnElapsed = 0f;
        isReturning = true;
    }

    public void SetOnTray(TrayCheck initialTray)
    {
        tray = initialTray;
        onTray = true;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = GetPointerPosition();

        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.layer == 5)
            {
                return true;
            }
        }
        return false;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float m = x - 1f;
        return 1f + c3 * m * m * m + c1 * m * m;
    }

    // INPUT HELPERS

    private bool IsPrimaryClickedThisFrame()
    {
        if (Touchscreen.current != null && (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld))
        {
            return Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }
        return false;
    }

    private bool IsPrimaryClickHeld()
    {
        if (Touchscreen.current != null && (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld))
        {
            return Touchscreen.current.primaryTouch.press.isPressed;
        }
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return true;
        }
        return false;
    }

    private bool IsPrimaryClickReleased()
    {
        if (Touchscreen.current != null && (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld))
        {
            return Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return true;
        }
        return false;
    }

    private Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null && (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld))
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        return Vector2.zero;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }

}