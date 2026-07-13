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

    // Methods

    void Awake()
    {
        cam = Camera.main;
        camZDepth = cam.WorldToScreenPoint(transform.position).z;
        trayLayer = LayerMask.NameToLayer("Tray");
        binLayer = LayerMask.NameToLayer("Bin");
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
            transform.position = Vector3.MoveTowards(transform.position, initialPos, returnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, initialPos) < 0.01f)
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

    // INPUT HELPERS

    private bool IsPrimaryClickedThisFrame()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }
        return false;
    }

    private bool IsPrimaryClickHeld()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return true;
        }
        return false;
    }

    private bool IsPrimaryClickReleased()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            return true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            return true;
        }
        return false;
    }

    private Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null)
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