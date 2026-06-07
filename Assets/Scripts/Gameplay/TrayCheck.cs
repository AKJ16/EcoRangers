using UnityEngine;

public class TrayCheck : MonoBehaviour
{
    // Attributes
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private AudioClip traySound;

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

    public bool HandleTrashDrop (GameObject trash, bool force = false)
    {
        if (currTrash != null && !force) 
        {
            return false;
        }

        if (trash.tag == "Composite")
        {
            CompositeTrash composite = trash.GetComponent<CompositeTrash>();
            composite.Split(this);
            return true;
        }

        currTrash = trash;
        trash.transform.position = transform.position + new Vector3(0, 0, -0.1f); ;

        if (AudioManager.Instance != null && traySound != null)
        {
            AudioManager.Instance.PlaySFX(traySound);
        }

        TrashItem item = trash.GetComponent<TrashItem>();
        if (item != null)
        {
            item.StopMoving();
        }

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

    public GameObject GetCurrentTrash()
    {
        return currTrash;
    }
}
