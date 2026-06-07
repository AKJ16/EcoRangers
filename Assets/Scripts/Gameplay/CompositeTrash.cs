using UnityEngine;

public class CompositeTrash : MonoBehaviour
{
    // Attributes

    [SerializeField] private GameObject[] parts;
    [SerializeField] private AudioClip splitSound;

    // Methods

    private void Start()
    {
        foreach (GameObject part in parts)
        {
            Collider2D col = part.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    public void Split(TrayCheck tray)
    {
        transform.position = tray.transform.position + new Vector3(0, 0, -0.1f);
        GameObject mainPart = parts[0];

        if (AudioManager.Instance != null && splitSound != null)
        {
            AudioManager.Instance.PlaySFX(splitSound);
        }

        for (int i = 0; i < parts.Length; i++)
        {
            GameObject part = parts[i];

            SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder += (i + 1);
            }

            if (i == 0)
            {
                part.transform.SetParent(null);
                if (sr != null) sr.sortingOrder += 1;
                part.transform.position = transform.position + new Vector3(0, 0, -0.01f);

                tray.HandleTrashDrop(part, true);
                part.GetComponent<DragDrop>().SetOnTray(tray);
            }
            else
            {
                part.transform.SetParent(mainPart.transform);
                if (sr != null) sr.sortingOrder += (i + 1);

                part.transform.localPosition = new Vector3(part.transform.localPosition.x, part.transform.localPosition.y, -0.01f * (i + 1));

                DragDrop dragDrop = part.GetComponent<DragDrop>();
                if (dragDrop != null) dragDrop.enabled = true;
            }

            Collider2D col = part.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = true;
            }

            TrashItem trash = part.GetComponent<TrashItem>();
            if (trash != null)
            {
                trash.StopMoving();
                trash.ItemFeedback();
            }
        }
        Destroy(gameObject);
    }
}
