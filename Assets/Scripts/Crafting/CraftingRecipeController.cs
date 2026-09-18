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
    [SerializeField] private Sprite woodenSwordSprite;
    [SerializeField] private Sprite woodenPickaxeSprite;
    [SerializeField] private Sprite woodenAxeSprite;

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

        if (craftSlots.Length != 9)
        {
            Debug.LogWarning(
                "CraftingRecipeController expected 9 crafting slots, but found "
                + craftSlots.Length
            );
        }

        RefreshRecipe();
    }

    public void RefreshRecipe()
    {
        if (craftSlots == null || craftSlots.Length < 9)
        {
            ClearOutput();
            return;
        }

        // =====================================================
        // SIMPLE RECIPE: 1 LOG -> 2 PLANKS
        // =====================================================
        if (HasExactlyOneItem(ItemType.Log))
        {
            SetOutput(
                ItemType.Plank,
                2,
                plankSprite
            );

            return;
        }

        // =====================================================
        // SIMPLE RECIPE: 1 PLANK -> 4 STICKS
        // =====================================================
        if (HasExactlyOneItem(ItemType.Plank))
        {
            SetOutput(
                ItemType.Stick,
                4,
                stickSprite
            );

            return;
        }

        // =====================================================
        // WOODEN SWORD
        //
        // [ ] [P] [ ]
        // [ ] [P] [ ]
        // [ ] [S] [ ]
        // =====================================================
        if (MatchesRecipe(
            new ItemType?[]
            {
                null, ItemType.Plank, null,
                null, ItemType.Plank, null,
                null, ItemType.Stick, null
            }))
        {
            SetOutput(
                ItemType.WoodenSword,
                1,
                woodenSwordSprite
            );

            return;
        }

        // =====================================================
        // WOODEN PICKAXE
        //
        // [P] [P] [P]
        // [ ] [S] [ ]
        // [ ] [S] [ ]
        // =====================================================
        if (MatchesRecipe(
            new ItemType?[]
            {
                ItemType.Plank, ItemType.Plank, ItemType.Plank,
                null,           ItemType.Stick, null,
                null,           ItemType.Stick, null
            }))
        {
            SetOutput(
                ItemType.WoodenPickaxe,
                1,
                woodenPickaxeSprite
            );

            return;
        }

        // =====================================================
        // WOODEN AXE - LEFT VERSION
        //
        // [P] [P] [ ]
        // [P] [S] [ ]
        // [ ] [S] [ ]
        // =====================================================
        if (MatchesRecipe(
            new ItemType?[]
            {
                ItemType.Plank, ItemType.Plank, null,
                ItemType.Plank, ItemType.Stick, null,
                null,           ItemType.Stick, null
            }))
        {
            SetOutput(
                ItemType.WoodenAxe,
                1,
                woodenAxeSprite
            );

            return;
        }

        // =====================================================
        // WOODEN AXE - MIRRORED VERSION
        //
        // [ ] [P] [P]
        // [ ] [S] [P]
        // [ ] [S] [ ]
        // =====================================================
        if (MatchesRecipe(
            new ItemType?[]
            {
                null, ItemType.Plank, ItemType.Plank,
                null, ItemType.Stick, ItemType.Plank,
                null, ItemType.Stick, null
            }))
        {
            SetOutput(
                ItemType.WoodenAxe,
                1,
                woodenAxeSprite
            );

            return;
        }

        // Nothing matched.
        ClearOutput();
    }

    // ---------------------------------------------------------
    // Checks if the crafting grid contains exactly ONE item
    // and that item is the requested type.
    // ---------------------------------------------------------
    private bool HasExactlyOneItem(ItemType requiredType)
    {
        int occupiedCount = 0;
        ItemType foundType = default;

        foreach (CraftingGridSlotUI slot in craftSlots)
        {
            if (slot == null || slot.IsEmpty)
                continue;

            occupiedCount++;
            foundType = slot.CurrentItemType;
        }

        return occupiedCount == 1 &&
               foundType == requiredType;
    }

    // ---------------------------------------------------------
    // Matches an exact 3x3 shaped recipe.
    //
    // null = slot must be empty
    // ItemType = slot must contain that exact item
    // ---------------------------------------------------------
    private bool MatchesRecipe(ItemType?[] pattern)
    {
        if (pattern == null || pattern.Length != 9)
            return false;

        if (craftSlots == null || craftSlots.Length < 9)
            return false;

        for (int i = 0; i < 9; i++)
        {
            ItemType? requiredItem = pattern[i];
            CraftingGridSlotUI slot = craftSlots[i];

            if (slot == null)
                return false;

            // This position should be empty.
            if (!requiredItem.HasValue)
            {
                if (!slot.IsEmpty)
                    return false;

                continue;
            }

            // This position requires an ingredient.
            if (slot.IsEmpty)
                return false;

            if (slot.CurrentItemType != requiredItem.Value)
                return false;
        }

        return true;
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

        // Add crafted item to the REAL inventory first.
        bool added =
            inventoryController.AddItem(
                resultType,
                resultQuantity
            );

        // Inventory full: do not consume ingredients.
        if (!added)
            return;

        // Craft succeeded.
        // Ingredients were already removed from the real inventory
        // when they were placed into these crafting slots,
        // so clearing these slots consumes them.
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