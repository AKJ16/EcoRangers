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

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartDrag();
        }

        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Drag();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
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

        if (IsPointerOverUI())
        {
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            initialPos = transform.position;
            offset = transform.position - GetMouseWorldPosition(mousePos);

            if (AudioManager.Instance != null && dragSound != null)
            {
                AudioManager.Instance.PlaySFX(dragSound);
            }
        }
    }

    void Drag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 trashPos = GetMouseWorldPosition(mousePos) + offset;
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
        eventData.position = Mouse.current.position.ReadValue();

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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }

}