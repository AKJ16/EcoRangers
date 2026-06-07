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
                CheckTrash();
            }
            isDragging = false;
        }
    }

    void StartDrag()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            initialPos = transform.position;
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
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
        transform.position = GetMouseWorldPosition(mousePos) + offset;
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
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
    }
}
