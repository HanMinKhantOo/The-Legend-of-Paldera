using UnityEngine;
using UnityEngine.UI;

public class CraftingInventoryView : MonoBehaviour
{
    [Header("Real Inventory")]
    [SerializeField] private InventoryController inventoryController;

    [Header("Materials Display")]
    [SerializeField] private CraftingInventorySlotUI[] materialSlots;

    [Header("Bottom Inventory Display")]
    [SerializeField] private CraftingInventorySlotUI[] displaySlots;

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        // Temporary live synchronization.
        Refresh();
    }

    public void Refresh()
    {
        if (inventoryController == null)
            return;

        if (inventoryController.inventoryPanel == null)
            return;

        Slot[] inventorySlots =
            inventoryController.inventoryPanel.GetComponentsInChildren<Slot>(true);

        PopulateDisplay(materialSlots, inventorySlots);
        PopulateDisplay(displaySlots, inventorySlots);
    }

    private void PopulateDisplay(
        CraftingInventorySlotUI[] targetSlots,
        Slot[] inventorySlots)
    {
        if (targetSlots == null)
            return;

        // Clear old display.
        foreach (CraftingInventorySlotUI targetSlot in targetSlots)
        {
            if (targetSlot != null)
            {
                targetSlot.ClearSlot();
            }
        }

        int displayIndex = 0;

        foreach (Slot inventorySlot in inventorySlots)
        {
            if (inventorySlot == null)
                continue;

            if (inventorySlot.currentItem == null)
                continue;

            while (
                displayIndex < targetSlots.Length &&
                targetSlots[displayIndex] == null)
            {
                displayIndex++;
            }

            if (displayIndex >= targetSlots.Length)
                break;

            InventoryItem item =
                inventorySlot.currentItem.GetComponent<InventoryItem>();

            if (item == null)
                continue;

            Sprite itemSprite =
                FindItemSprite(inventorySlot.currentItem);

            if (itemSprite == null)
                continue;

            targetSlots[displayIndex].SetItem(
                itemSprite,
                item.quantity,
                item.itemType
            );

            displayIndex++;
        }
    }

    private Sprite FindItemSprite(GameObject itemObject)
    {
        Image[] images =
            itemObject.GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            if (image != null && image.sprite != null)
            {
                return image.sprite;
            }
        }

        return null;
    }
}