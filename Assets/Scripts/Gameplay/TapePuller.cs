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

        if (IsPrimaryClickedThisFrame())
        {
            Vector2 pointerPos = GetPointerPosition();
            RaycastHit2D hit = Physics2D.GetRayIntersection(cam.ScreenPointToRay(pointerPos));
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
            }
        }

        if (isDragging && IsPrimaryClickHeld())
        {
            Vector2 pointerPos = GetPointerPosition();
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, 10f));
            Vector3 localPos = boxTransform.InverseTransformPoint(worldPos);

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

        if (IsPrimaryClickReleased())
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
}
