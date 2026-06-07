using UnityEngine;

public class ItemDependency : MonoBehaviour
{
    // Attributes

    [SerializeField] private GameObject[] requiredRemovedItems;
    private DragDrop dragDrop;
    private BoxLogic boxLogic;

    // Methods

    private void Awake()
    {
        dragDrop = GetComponent<DragDrop>();
        boxLogic = GetComponent<BoxLogic>();
    }

    private void Update()
    {
        if (dragDrop != null)
        {
            bool itemsCleared = IsUnlocked();

            if (boxLogic != null)
            {
                dragDrop.enabled = itemsCleared && boxLogic.GetIsFlattened();
            }
            else
            {
                dragDrop.enabled = itemsCleared;
            }
        }
            
    }

    public bool IsUnlocked()
    {
        foreach (GameObject item in requiredRemovedItems)
        {
            if (item != null) return false;
        }
        return true;
    }

    public void UpdateRequirements(GameObject[] newItems)
    {
        requiredRemovedItems = newItems;
    }
}
