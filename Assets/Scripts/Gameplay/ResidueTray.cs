using UnityEngine;
using UnityEngine.InputSystem;

public class ResidueTray : MonoBehaviour
{
    // Attributes

    [SerializeField] private Sprite processedSprite;
    [SerializeField] public Vector3 processedScale;
    [SerializeField] public float processedRotation;
    [SerializeField] private string requiredTag;
    [SerializeField] private string processTag;
    [SerializeField] private TrayCheck mainTray;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float snapDistance = 1.0f;
    [SerializeField] private AudioClip dragSound;
    [SerializeField] private AudioClip releaseSound;

    private Vector3 initialPos;
    private float camZDepth;
    private bool isDragging;
    private Vector3 offset;
    private Camera cam;
    private bool isReturning;

    // Methods

    private void Awake()
    {
        cam = Camera.main;
        initialPos = transform.position;
        camZDepth = cam.WorldToScreenPoint(transform.position).z;
    }

    private void Update()
    {
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
                CheckTrash();
            }
            isDragging = false;
        }
    }

    void StartDrag()
    {
        Vector2 pointerPos = GetPointerPosition();
        Ray ray = cam.ScreenPointToRay(pointerPos);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            initialPos = transform.position;
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
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
        transform.position = GetMouseWorldPosition(pointerPos) + offset;
    }

    Vector3 GetMouseWorldPosition(Vector2 screenPos)
    {
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, camZDepth));
    }

    private void CheckTrash()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, snapDistance);

        foreach (var hit in hits)
        {
            TrashItem trash = hit.GetComponent<TrashItem>();

            if (trash != null && trash.gameObject.tag == requiredTag)
            {

                if (mainTray != null)
                {
                    GameObject mainTrash = mainTray.GetCurrentTrash();

                    if (mainTrash == trash.gameObject || mainTrash == null)
                    {
                        trash.gameObject.tag = processTag;
                        trash.ProcessSprite(processedSprite, processedScale, processedRotation);
                        trash.StopMoving();

                        mainTray.HandleTrashDrop(trash.gameObject, true);
                        transform.position = initialPos;

                        if (AudioManager.Instance != null && releaseSound != null)
                        {
                            AudioManager.Instance.PlaySFX(releaseSound);
                        }
                        break;
                    }
                }
            } 
        }
        isReturning = true;
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
}
