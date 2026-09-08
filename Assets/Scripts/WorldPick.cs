using UnityEngine;

public class WorldPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType;
    [SerializeField] private int amount = 1;

    private InventoryController inventoryController;
    private bool pickedUp = false;

    private void Start()
    {
        inventoryController = FindAnyObjectByType<InventoryController>();

        if (inventoryController == null)
        {
            Debug.LogError("WorldPickup could not find InventoryController.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (pickedUp)
            return;

        // Accept the Player itself or a collider belonging to the Player.
        if (!other.CompareTag("Player") &&
            !other.transform.root.CompareTag("Player"))
        {
            return;
        }

        TryPickup();
    }

    private void TryPickup()
    {
        if (inventoryController == null)
            return;

        bool added = inventoryController.AddItem(itemType, amount);

        if (added)
        {
            pickedUp = true;
            Destroy(gameObject);
        }
    }
}