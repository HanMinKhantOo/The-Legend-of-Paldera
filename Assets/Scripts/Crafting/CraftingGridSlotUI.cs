using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftingGridSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private InventoryController inventoryController;

    private CraftingRecipeController recipeController;

    public Sprite CurrentIcon { get; private set; }
    public ItemType CurrentItemType { get; private set; }

    public bool IsEmpty { get; private set; } = true;

    private void Awake()
    {
        // Find the Icon child automatically.
        if (itemIcon == null)
        {
            Transform iconTransform = transform.Find("Icon");

            if (iconTransform != null)
            {
                itemIcon = iconTransform.GetComponent<Image>();
            }
        }

        // Find the real inventory automatically.
        if (inventoryController == null)
        {
            inventoryController =
                FindFirstObjectByType<InventoryController>();
        }

        // Find the recipe controller on CraftingPage.
        recipeController =
            GetComponentInParent<CraftingRecipeController>();

        ClearSlot();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryController == null)
        {
            Debug.LogError(
                "CraftingGridSlotUI could not find InventoryController."
            );

            return;
        }

        // ---------------------------------
        // OCCUPIED SLOT:
        // Return ingredient to inventory.
        // ---------------------------------
        if (!IsEmpty)
        {
            bool returned =
                inventoryController.AddItem(
                    CurrentItemType,
                    1
                );

            if (returned)
            {
                ClearSlot();
            }

            return;
        }

        // ---------------------------------
        // EMPTY SLOT:
        // Nothing selected = do nothing.
        // ---------------------------------
        if (!CraftingInventorySlotUI.HasSelection)
            return;

        ItemType selectedType =
            CraftingInventorySlotUI.SelectedItemType;

        // Remove exactly one real item.
        bool removed =
            inventoryController.TryRemoveItem(
                selectedType,
                1
            );

        // Player doesn't own enough.
        if (!removed)
            return;

        // Put selected ingredient into this slot.
        SetItem(
            CraftingInventorySlotUI.SelectedIcon,
            selectedType
        );
    }

    public void SetItem(
        Sprite icon,
        ItemType itemType
    )
    {
        CurrentIcon = icon;
        CurrentItemType = itemType;
        IsEmpty = false;

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
            itemIcon.preserveAspect = true;
        }

        // Recheck crafting recipe.
        if (recipeController != null)
        {
            recipeController.RefreshRecipe();
        }
    }

    public void ClearSlot()
    {
        CurrentIcon = null;
        CurrentItemType = default;
        IsEmpty = true;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        // Recheck crafting recipe.
        if (recipeController != null)
        {
            recipeController.RefreshRecipe();
        }
    }
}