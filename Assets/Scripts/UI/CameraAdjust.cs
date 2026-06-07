using UnityEngine;

[ExecuteInEditMode]
public class CameraAdjust : MonoBehaviour
{
    [SerializeField] private float defaultOrthoSize = 5f;
    [SerializeField] private float targetAspectWidth = 16f;
    [SerializeField] private float targetAspectHeight = 9f;

    private Camera cam;
    private float targetAspect;
    private float targetWidth;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        InitializeBounds();
    }

    private void InitializeBounds()
    {
        targetAspect = targetAspectWidth / targetAspectHeight;
        targetWidth = (defaultOrthoSize * 2f) * targetAspect;
    }

    private void LateUpdate()
    {
        AdjustCamera();
    }

    private void AdjustCamera()
    {
        if (cam == null) cam = GetComponent<Camera>();

        float currentAspect = (float)Screen.width / Screen.height;

#if UNITY_EDITOR
        InitializeBounds();
#endif

        if (currentAspect < targetAspect)
        {
            
            float unitsPerPixel = targetWidth / Screen.width;
            cam.orthographicSize = 0.5f * unitsPerPixel * Screen.height;
        }
        else
        {
            cam.orthographicSize = defaultOrthoSize;
        }
    }
}