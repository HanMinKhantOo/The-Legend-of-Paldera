using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Inventory Setup")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount = 40;

    [Header("Item Prefabs")]
    public GameObject branchItemPrefab;
    public GameObject stoneItemPrefab;
    public GameObject logItemPrefab;
    public GameObject foodItemPrefab;

    public GameObject plankItemPrefab;
    public GameObject stickItemPrefab;

    public GameObject woodenSwordItemPrefab;
    public GameObject woodenPickaxeItemPrefab;
    public GameObject woodenAxeItemPrefab;

    [Header("Gem Prefabs (Altar Activation)")]
    public GameObject gemRedItemPrefab;
    public GameObject gemGreenItemPrefab;
    public GameObject gemBlueItemPrefab;
    public GameObject gemYellowItemPrefab;

    [Header("Gem Prefabs (Boss Drop / Ending)")]
    public GameObject gemRainbowItemPrefab;

    [Header("Mining Prefabs")]
    public GameObject ironBarItemPrefab;

    private List<Slot> slots = new List<Slot>();

    private void Start()
    {
        CreateSlots();
    }

    private void CreateSlots()
    {
        slots.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObject = Instantiate(
                slotPrefab,
                inventoryPanel.transform
            );

            Slot slot = slotObject.GetComponent<Slot>();

            if (slot != null)
            {
                slots.Add(slot);
            }
        }
    }

    public bool AddItem(ItemType itemType, int amount = 1)
    {
        SyncSlotItemReferences();
        // First try to add to an existing stack.
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null)
                continue;

            InventoryItem existingItem =
                slot.currentItem.GetComponent<InventoryItem>();

            if (existingItem == null)
                continue;

            if (existingItem.itemType == itemType &&
                existingItem.CanAddMore())
            {
                int availableSpace =
                    existingItem.maxStack - existingItem.quantity;

                int amountToAdd =
                    Mathf.Min(amount, availableSpace);

                existingItem.AddQuantity(amountToAdd);

                amount -= amountToAdd;

                if (amount <= 0)
                {
                    return true;
                }
            }
        }

        // Create new stacks if items are still remaining.
        while (amount > 0)
        {
            Slot emptySlot = FindEmptySlot();

            if (emptySlot == null)
            {
                Debug.Log("Inventory is full!");
                return false;
            }

            GameObject prefab = GetItemPrefab(itemType);

            if (prefab == null)
            {
                Debug.LogError(
                    "No inventory prefab assigned for: " + itemType
                );

                return false;
            }

            GameObject newItem =
                Instantiate(prefab, emptySlot.transform);

            RectTransform rect =
                newItem.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
            }

            InventoryItem inventoryItem =
                newItem.GetComponent<InventoryItem>();

            if (inventoryItem == null)
            {
                Debug.LogError(
                    prefab.name +
                    " does not contain an InventoryItem component!"
                );

                Destroy(newItem);
                return false;
            }

            int amountForThisStack =
                Mathf.Min(amount, inventoryItem.maxStack);

            inventoryItem.SetQuantity(amountForThisStack);

            emptySlot.currentItem = newItem;

            amount -= amountForThisStack;
        }

        return true;
    }

    private Slot FindEmptySlot()
    {
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null)
            {
                return slot;
            }
        }

        return null;
    }
    private void SyncSlotItemReferences()
    {
        foreach (Slot slot in slots)
        {
            if (slot == null)
                continue;

            // Already connected correctly.
            if (slot.currentItem != null)
                continue;

            // Check whether an item already exists visually inside this slot.
            InventoryItem existingItem =
                slot.GetComponentInChildren<InventoryItem>(true);

            if (existingItem != null)
            {
                slot.currentItem = existingItem.gameObject;
            }
        }
    }

    private GameObject GetItemPrefab(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Branch:
                return branchItemPrefab;

            case ItemType.Stone:
                return stoneItemPrefab;

            case ItemType.Log:
                return logItemPrefab;

            case ItemType.Food:
                return foodItemPrefab;

            case ItemType.Plank:
                return plankItemPrefab;

            case ItemType.Stick:
                return stickItemPrefab;

            case ItemType.WoodenSword:
                return woodenSwordItemPrefab;

            case ItemType.WoodenPickaxe:
                return woodenPickaxeItemPrefab;

            case ItemType.WoodenAxe:
                return woodenAxeItemPrefab;

            case ItemType.GemRed:
                return gemRedItemPrefab;

            case ItemType.GemGreen:
                return gemGreenItemPrefab;

            case ItemType.GemBlue:
                return gemBlueItemPrefab;

            case ItemType.GemYellow:
                return gemYellowItemPrefab;

            case ItemType.GemRainbow:
                return gemRainbowItemPrefab;

            case ItemType.IronBar:
                return ironBarItemPrefab;

            default:
                Debug.LogError(
                    "Unsupported ItemType: " + itemType
                );
                return null;
        }
    }

    /// <summary>
    /// Removes one item instance (a whole slot's stack, or a single unit off
    /// a stack) - used by FoodConsumable when the player eats. Kept here
    /// since InventoryController already owns slot bookkeeping.
    /// </summary>
    public void ConsumeOneFromSlot(Slot slot, InventoryItem item)
    {
        if (slot == null || item == null) return;

        if (item.quantity > 1)
        {
            item.SetQuantity(item.quantity - 1);
        }
        else
        {
            slot.currentItem = null;
            Destroy(item.gameObject);
        }
    }
    public int GetItemCount(ItemType itemType)
    {
        SyncSlotItemReferences();

        int total = 0;

        foreach (Slot slot in slots)
        {
            if (slot == null || slot.currentItem == null)
                continue;

            InventoryItem item =
                slot.currentItem.GetComponent<InventoryItem>();

            if (item == null)
                continue;

            if (item.itemType == itemType)
            {
                total += item.quantity;
            }
        }
        return total;
    }

    public bool TryRemoveItem(ItemType itemType, int amount = 1)
    {
        if (amount <= 0)
            return true;

        SyncSlotItemReferences();

        // First make sure we actually own enough.
        if (GetItemCount(itemType) < amount)
        {
            Debug.Log(
                "Not enough " + itemType +
                " in inventory."
            );

            return false;
        }

        int remaining = amount;

        foreach (Slot slot in slots)
        {
            if (slot == null || slot.currentItem == null)
                continue;

            InventoryItem item =
                slot.currentItem.GetComponent<InventoryItem>();

            if (item == null)
                continue;

            if (item.itemType != itemType)
                continue;

            int removeFromThisStack =
                Mathf.Min(remaining, item.quantity);

            int newQuantity =
                item.quantity - removeFromThisStack;

            if (newQuantity > 0)
            {
                item.SetQuantity(newQuantity);
            }
            else
            {
                GameObject itemObject = slot.currentItem;

                slot.currentItem = null;

                Destroy(itemObject);
            }

            remaining -= removeFromThisStack;

            if (remaining <= 0)
                return true;
        }
        return false;
    }
}