using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private Image outputIcon;
    [SerializeField] private TextMeshProUGUI outputQuantityText;

    [Header("Crafted Item Sprites")]
    [SerializeField] private Sprite plankSprite;
    [SerializeField] private Sprite stickSprite;

    private CraftingGridSlotUI[] craftSlots;

    private bool hasValidRecipe = false;
    private ItemType resultType;
    private int resultQuantity;

    private void Start()
    {
        if (inventoryController == null)
        {
            inventoryController =
                FindFirstObjectByType<InventoryController>();
        }

        craftSlots =
            GetComponentsInChildren<CraftingGridSlotUI>(true);

        RefreshRecipe();
    }

    public void RefreshRecipe()
    {
        if (craftSlots == null)
            return;

        CraftingGridSlotUI ingredientSlot = null;
        int occupiedSlots = 0;

        foreach (CraftingGridSlotUI slot in craftSlots)
        {
            if (slot == null || slot.IsEmpty)
                continue;

            occupiedSlots++;
            ingredientSlot = slot;
        }

        // These first recipes require exactly ONE ingredient.
        if (occupiedSlots != 1 || ingredientSlot == null)
        {
            ClearOutput();
            return;
        }

        switch (ingredientSlot.CurrentItemType)
        {
            // 1 Log -> 2 Planks
            case ItemType.Log:
                SetOutput(
                    ItemType.Plank,
                    2,
                    plankSprite
                );
                break;

            // 1 Plank -> 4 Sticks
            case ItemType.Plank:
                SetOutput(
                    ItemType.Stick,
                    4,
                    stickSprite
                );
                break;

            default:
                ClearOutput();
                break;
        }
    }

    private void SetOutput(
        ItemType itemType,
        int quantity,
        Sprite sprite
    )
    {
        resultType = itemType;
        resultQuantity = quantity;
        hasValidRecipe = true;

        if (outputIcon != null)
        {
            outputIcon.sprite = sprite;
            outputIcon.enabled = sprite != null;
            outputIcon.preserveAspect = true;
        }

        if (outputQuantityText != null)
        {
            outputQuantityText.text =
                quantity.ToString();
        }
    }

    private void ClearOutput()
    {
        hasValidRecipe = false;
        resultQuantity = 0;

        if (outputIcon != null)
        {
            outputIcon.sprite = null;
            outputIcon.enabled = false;
        }

        if (outputQuantityText != null)
        {
            outputQuantityText.text = "";
        }
    }

    public void TryCraft()
    {
        if (!hasValidRecipe)
            return;

        if (inventoryController == null)
            return;

        bool added =
            inventoryController.AddItem(
                resultType,
                resultQuantity
            );

        if (!added)
            return;

        foreach (CraftingGridSlotUI slot in craftSlots)
        {
            if (slot != null && !slot.IsEmpty)
            {
                slot.ClearSlot();
            }
        }

        ClearOutput();
    }
}