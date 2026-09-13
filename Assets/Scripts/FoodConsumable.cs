using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach alongside InventoryItem on food item prefabs. Double-click a food
/// item in the inventory to eat one unit of it, restoring Hunger (and
/// optionally HP) via PlayerVitals. Only does anything when this item's
/// InventoryItem.itemType is Food - safe to leave off non-food prefabs.
///
/// Added as a new component rather than baking eat-logic into DragDrop.cs so
/// the existing drag/reorder behavior is untouched.
/// </summary>
[RequireComponent(typeof(InventoryItem))]
public class FoodConsumable : MonoBehaviour, IPointerClickHandler
{
    private InventoryItem item;

    private void Awake()
    {
        item = GetComponent<InventoryItem>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount < 2) return; // require double-click
        if (item.itemType != ItemType.Food) return;

        if (PlayerVitals.Instance == null)
        {
            Debug.LogWarning("[FoodConsumable] No PlayerVitals in the scene - cannot eat.");
            return;
        }

        PlayerVitals.Instance.Eat(item.hungerRestore, item.hpRestore);

        Slot parentSlot = GetComponentInParent<Slot>();
        InventoryController inventoryController = FindObjectOfType<InventoryController>();
        if (inventoryController != null)
        {
            inventoryController.ConsumeOneFromSlot(parentSlot, item);
        }
        else
        {
            // Fallback if no InventoryController is found in the scene.
            if (item.quantity > 1) item.SetQuantity(item.quantity - 1);
            else Destroy(gameObject);
        }
    }
}
