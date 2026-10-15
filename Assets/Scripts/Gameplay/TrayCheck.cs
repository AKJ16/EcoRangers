using System.Collections.Generic;
using UnityEngine;

public class TrayCheck : MonoBehaviour
{
    // Attributes
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private AudioClip traySound;

    [Header("VFX")]
    [SerializeField] private bool enableVFX = true;
    [SerializeField] private float vfxScale = 1f;
    [SerializeField] private int vfxSortingBoost = 20;
    [SerializeField] private string compositeTag = "Composite";
    [SerializeField] private List<string> complexTags = new List<string> { "B3", "Residue"};

    private GameObject currTrash;
    private Vector3 originalScale;
    private bool isPulsing;
    private SpriteRenderer trayRenderer;

    // Methods

    private void Awake()
    {
        originalScale = transform.localScale;
        trayRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isPulsing)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale * pulseScale, pulseSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.localScale, originalScale * pulseScale) < 0.05f)
            {
                isPulsing = false;
            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, pulseSpeed * Time.deltaTime);
        }
    }

    public bool HandleTrashDrop(GameObject trash, bool force = false)
    {
        if (currTrash != null && !force)
        {
            return false;
        }

        // 1. Composite Trash: Yellow-orangish burst + ring
        if (trash.CompareTag(compositeTag) || trash.GetComponent<CompositeTrash>() != null)
        {
            if (enableVFX)
            {
                SimpleVFX.TrayComposite(transform.position, vfxScale, GetVFXReference(trash), vfxSortingBoost);
            }

            CompositeTrash composite = trash.GetComponent<CompositeTrash>();
            if (composite != null)
            {
                composite.Split(this);
            }
            return true;
        }

        currTrash = trash;
        trash.transform.position = transform.position + new Vector3(0, 0, -0.1f);

        if (AudioManager.Instance != null && traySound != null)
        {
            AudioManager.Instance.PlaySFX(traySound);
        }

        TrashItem item = trash.GetComponent<TrashItem>();
        if (item != null)
        {
            item.StopMoving();
        }

        // Play VFX on drop (ignore internal split re-drops where force == true)
        if (enableVFX && !force)
        {
            if (IsComplexTrash(trash))
            {
                // 2. Complex Trash (B3 or Residue): White burst + ring
                SimpleVFX.TrayComplex(transform.position, vfxScale, GetVFXReference(trash), vfxSortingBoost);
            }
            else
            {
                // 3. Regular Trash: Simple white expanding ring pulse
                SimpleVFX.TraySimplePulse(transform.position, vfxScale, GetVFXReference(trash), vfxSortingBoost);
            }
        }

        return true;
    }

    public void PlayComplexVFX(Vector3? customPos = null)
    {
        if (!enableVFX) return;
        Vector3 pos = customPos ?? transform.position;
        SimpleVFX.TrayComplex(pos, vfxScale, trayRenderer, vfxSortingBoost);
    }

    private bool IsComplexTrash(GameObject trash)
    {
        if (trash == null) return false;
        for (int i = 0; i < complexTags.Count; i++)
        {
            if (trash.CompareTag(complexTags[i])) return true;
        }
        return false;
    }

    private SpriteRenderer GetVFXReference(GameObject trash)
    {
        if (trayRenderer != null) return trayRenderer;
        return trash != null ? trash.GetComponent<SpriteRenderer>() : null;
    }

    public void FreeTray()
    {
        currTrash = null;
    }

    public void Pulse()
    {
        isPulsing = true;
    }

    public GameObject GetCurrentTrash()
    {
        return currTrash;
    }
}