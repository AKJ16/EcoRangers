using UnityEngine;
using UnityEngine.InputSystem;

public class TapePuller : MonoBehaviour
{
    // Attributes

    [SerializeField] private Transform boxTransform;
    [SerializeField] private float startY = 1.7f;
    [SerializeField] private float endY = -1.7f;
    [SerializeField] private float completionThreshold = 0.9f;

    private Camera cam;
    private bool isDragging;
    private BoxLogic boxLogic;

    // Methods

    private void Awake()
    {
        cam = Camera.main;
        boxLogic = boxTransform.GetComponent<BoxLogic>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0.99f) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            RaycastHit2D hit = Physics2D.GetRayIntersection(cam.ScreenPointToRay(mousePos));
            if (hit.collider != null && hit.collider.gameObject == gameObject)
                isDragging = true;
        }

        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 10f));
            Vector3 localPos = boxTransform.InverseTransformPoint(mouseWorldPos);

            float absoluteMin = Mathf.Min(startY, endY);
            float absoluteMax = Mathf.Max(startY, endY);

            float clampedY = Mathf.Clamp(localPos.y, absoluteMin, absoluteMax);
            transform.localPosition = new Vector3(0, clampedY, transform.localPosition.z);

            float progress = Mathf.InverseLerp(startY, endY, clampedY);

            if (progress >= completionThreshold)
            {
                FinishPulling();
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void FinishPulling()
    {
        isDragging = false;
        boxLogic.UnsealBox();
        Destroy(gameObject);
    }
}
