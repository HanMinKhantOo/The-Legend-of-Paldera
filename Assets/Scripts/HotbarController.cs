using UnityEngine;

public class HotbarController : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private InventoryController inventoryController;

    [Header("Hotbar Slots")]
    [SerializeField] private HotbarSlotUI[] slots;

    [Header("Item Sprites")]
    [SerializeField] private Sprite woodenSwordSprite;
    [SerializeField] private Sprite woodenPickaxeSprite;
    [SerializeField] private Sprite woodenAxeSprite;
    [SerializeField] private Sprite foodSprite;

    [Header("Player Equipment Animation")]
    [SerializeField] private Animator playerAnimator;

    [Tooltip("Your normal Player.controller")]
    [SerializeField] private RuntimeAnimatorController defaultPlayerController;

    [SerializeField] private AnimatorOverrideController woodenSwordOverride;
    [SerializeField] private AnimatorOverrideController woodenPickaxeOverride;
    [SerializeField] private AnimatorOverrideController woodenAxeOverride;

    private int selectedSlot = 0;

    private readonly ItemType[] slotItemTypes =
    {
        ItemType.WoodenSword,
        ItemType.WoodenPickaxe,
        ItemType.WoodenAxe,
        ItemType.Food
    };

    private void Start()
    {
        // Safety fallback.
        // If you forget to assign the normal Player controller,
        // grab whatever controller the Animator starts with.
        if (playerAnimator != null && defaultPlayerController == null)
        {
            defaultPlayerController = playerAnimator.runtimeAnimatorController;
        }

        SelectSlot(0);
        RefreshHotbar();
        RefreshEquipmentAnimator();
    }

    private void Update()
    {
        HandleNumberKeys();

        RefreshHotbar();

        // This also catches situations where an item is crafted/removed
        // while its hotbar slot is already selected.
        RefreshEquipmentAnimator();
    }

    private void HandleNumberKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectSlot(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectSlot(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectSlot(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SelectSlot(3);

        if (Input.GetKeyDown(KeyCode.Alpha5))
            SelectSlot(4);

        if (Input.GetKeyDown(KeyCode.Alpha6))
            SelectSlot(5);

        if (Input.GetKeyDown(KeyCode.Alpha7))
            SelectSlot(6);

        if (Input.GetKeyDown(KeyCode.Alpha8))
            SelectSlot(7);
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        selectedSlot = index;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].SetSelected(i == selectedSlot);
            }
        }

        if (TryGetSelectedItem(out ItemType itemType))
        {
            Debug.Log("Selected hotbar item: " + itemType);
        }
        else
        {
            Debug.Log("Selected empty hotbar slot.");
        }

        RefreshEquipmentAnimator();
    }

    private void RefreshHotbar()
    {
        if (inventoryController == null)
            return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            // Slots 5-8 are empty for now.
            if (i >= slotItemTypes.Length)
            {
                slots[i].ClearSlot();
                continue;
            }

            ItemType itemType = slotItemTypes[i];
            int quantity = inventoryController.GetItemCount(itemType);

            if (quantity <= 0)
            {
                slots[i].ClearSlot();
                continue;
            }

            Sprite sprite = GetSprite(itemType);
            slots[i].SetItem(sprite, quantity);
        }
    }

    private Sprite GetSprite(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.WoodenSword:
                return woodenSwordSprite;

            case ItemType.WoodenPickaxe:
                return woodenPickaxeSprite;

            case ItemType.WoodenAxe:
                return woodenAxeSprite;

            case ItemType.Food:
                return foodSprite;

            default:
                return null;
        }
    }

    private void RefreshEquipmentAnimator()
    {
        if (playerAnimator == null)
            return;

        if (defaultPlayerController == null)
            return;

        // Default = player is holding nothing.
        RuntimeAnimatorController targetController =
            defaultPlayerController;

        if (TryGetSelectedItem(out ItemType itemType))
        {
            switch (itemType)
            {
                case ItemType.WoodenSword:
                    if (woodenSwordOverride != null)
                        targetController = woodenSwordOverride;
                    break;

                case ItemType.WoodenPickaxe:
                    if (woodenPickaxeOverride != null)
                        targetController = woodenPickaxeOverride;
                    break;

                case ItemType.WoodenAxe:
                    if (woodenAxeOverride != null)
                        targetController = woodenAxeOverride;
                    break;

                // Food or anything else uses normal player animations.
                default:
                    targetController = defaultPlayerController;
                    break;
            }
        }

        // Only change controller when necessary.
        if (playerAnimator.runtimeAnimatorController != targetController)
        {
            playerAnimator.runtimeAnimatorController = targetController;
        }
    }

    public bool TryGetSelectedItem(out ItemType itemType)
    {
        itemType = default;

        if (selectedSlot < 0 ||
            selectedSlot >= slotItemTypes.Length ||
            inventoryController == null)
        {
            return false;
        }

        ItemType selectedType = slotItemTypes[selectedSlot];

        if (inventoryController.GetItemCount(selectedType) <= 0)
            return false;

        itemType = selectedType;
        return true;
    }

    public int GetSelectedSlot()
    {
        return selectedSlot;
    }
}