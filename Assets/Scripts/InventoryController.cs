using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Inventory Setup")]
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount = 40;

    [Header("Item Prefabs")]
    public GameObject[] itemPrefabs;

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

        // If there are items remaining, create new stacks.
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
                    "No inventory prefab found for: " + itemType
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
            }

            InventoryItem inventoryItem =
                newItem.GetComponent<InventoryItem>();

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

    private GameObject GetItemPrefab(ItemType itemType)
    {
        foreach (GameObject prefab in itemPrefabs)
        {
            if (prefab == null)
                continue;

            InventoryItem inventoryItem =
                prefab.GetComponent<InventoryItem>();

            if (inventoryItem != null &&
                inventoryItem.itemType == itemType)
            {
                return prefab;
            }
        }

        return null;
    }
}