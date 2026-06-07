using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoxLogic : MonoBehaviour
{
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite flattenedSprite;
    [SerializeField] private GameObject peeledTapePrefab;
    [SerializeField] private Transform tapeSpawnPoint;
    [SerializeField] private int clicksToFlatten = 3;
    [SerializeField] private string paperTag = "Paper";
    [SerializeField] private float clickPulse = 1.1f;
    [SerializeField] private float flattenShrink = 0.7f;
    [SerializeField] private AudioClip boxSound;

    private int currentClicks = 0;
    private bool isUnsealed = false;
    private bool isFlattened = false;
    private ItemDependency dependency;
    private TrashItem trashItem;
    private Vector3 originalScale;
    private float originalRotation;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
        dependency = GetComponent<ItemDependency>();
        trashItem = GetComponent<TrashItem>();
        originalScale = transform.localScale;
        originalRotation = transform.parent.eulerAngles.z;
    }

    private void Update()
    {
        if (Time.timeScale <= 0.99f) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                OnBoxClick();
            }
        }
    }

    public void UnsealBox()
    {
        isUnsealed = true;
        trashItem.ProcessSprite(openSprite, transform.localScale, originalRotation);

        if (AudioManager.Instance != null && boxSound != null)
        {
            AudioManager.Instance.PlaySFX(boxSound);
        }

        originalScale = transform.localScale;
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.05f);

        GameObject newTape = Instantiate(peeledTapePrefab, tapeSpawnPoint.position, tapeSpawnPoint.rotation);
        TrashItem tapeItem = newTape.GetComponent<TrashItem>();
        if (tapeItem != null)
        {
            tapeItem.ItemFeedback();
        }

        SpriteRenderer boxSR = GetComponent<SpriteRenderer>();
        SpriteRenderer tapeSR = newTape.GetComponent<SpriteRenderer>(); // Prefab likely has SR on root

        if (boxSR != null && tapeSR != null)
        {
            tapeSR.sortingOrder = boxSR.sortingOrder + 1;
        }

        newTape.transform.position = new Vector3(newTape.transform.position.x, newTape.transform.position.y, transform.position.z - 0.01f);

        if (dependency != null)
        {
            dependency.UpdateRequirements(new GameObject[] { newTape });
        }
    }

    private void OnBoxClick()
    {

        if (!isUnsealed || isFlattened) return;

        if (dependency != null && !dependency.IsUnlocked())
        {
            return;
        }

        currentClicks++;

        StopAllCoroutines();
        StartCoroutine(ClickPulseRoutine());

        if (currentClicks >= clicksToFlatten)
        {
            Flatten();
        }
    }

    private IEnumerator ClickPulseRoutine()
    {
        transform.localScale = originalScale * clickPulse;
        float elapsed = 0;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    private void Flatten()
    {
        isFlattened = true;
        transform.localScale = originalScale;
        originalScale = transform.localScale * flattenShrink;
        gameObject.tag = paperTag;
        trashItem.ProcessSprite(flattenedSprite, originalScale, originalRotation);

        if (AudioManager.Instance != null && boxSound != null)
        {
            AudioManager.Instance.PlaySFX(boxSound);
        }
    }

    public bool GetIsFlattened()
    {
        return isFlattened;
    }
}