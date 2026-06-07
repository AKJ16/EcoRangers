using UnityEngine;

public class ProcessingTray : MonoBehaviour
{
    // Attributes

    [SerializeField] private Sprite processedSprite;
    [SerializeField] public Vector3 processedScale;
    [SerializeField] public float processedRotation;
    [SerializeField] private string requiredTag;
    [SerializeField] private string processTag;
    [SerializeField] private TrayCheck mainTray;
    [SerializeField] private AudioClip processSound;

    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 10f;

    private GameObject currTrash;
    private Vector3 originalScale;
    private bool isPulsing;

    // Methods

    private void Awake()
    {
        originalScale = transform.localScale;
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

    public bool HandleTrashDrop(GameObject trash, TrashItem item)
    {
        if (currTrash != null)
            return false;

        if (trash.tag != requiredTag)
            return false;

        currTrash = trash;

        if (mainTray != null)
        {
            GameObject mainTrayTrash = mainTray.GetCurrentTrash();

            if (mainTrayTrash == trash || mainTrayTrash == null)
            {
                mainTray.HandleTrashDrop(trash, true);
            }
            else
            {
                currTrash = null;
                return false;
            }
        }

        trash.tag = processTag;
        if (item != null)
        {
            item.ProcessSprite(processedSprite, processedScale, processedRotation);
        }

        if (AudioManager.Instance != null && processSound != null)
        {
            AudioManager.Instance.PlaySFX(processSound);
        }

        Pulse();
        return true;
    }

    public void FreeTray()
    {
        currTrash = null;
    }

    public void Pulse()
    {
        isPulsing = true;
    }
}
