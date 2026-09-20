using UnityEngine;

/// <summary>
/// Reworked to be a real drag-and-drop hotbar instead of a fixed readout.
///
/// Previously this hardcoded which ItemType lived in each slot
/// (slotItemTypes[]), so items always landed in the same slot and couldn't
/// be dragged elsewhere. Now each hotbar slot GameObject just needs a
/// Slot component (the same one-line class inventory slots already use) -
/// the existing DragDrop.cs on every item prefab already knows how to find
/// and drop into any Slot, inventory or hotbar, with zero changes needed
/// there. This script just reads whatever InventoryItem is currently
/// sitting in the selected Slot.
/// </summary>
public class HotbarController : MonoBehaviour
{
    [Header("Hotbar Slots")]
    [Tooltip("Each element needs a Slot component (for drag/drop) alongside its HotbarSlotUI (for the selection tint).")]
    [SerializeField] private Slot[] slots;
    [SerializeField] private HotbarSlotUI[] slotVisuals;

    [Header("Player Equipment Animation")]
    [SerializeField] private Animator playerAnimator;

    [Tooltip("Your normal Player.controller")]
    [SerializeField] private RuntimeAnimatorController defaultPlayerController;

    [SerializeField] private AnimatorOverrideController woodenSwordOverride;
    [SerializeField] private AnimatorOverrideController woodenPickaxeOverride;
    [SerializeField] private AnimatorOverrideController woodenAxeOverride;

    private int selectedSlot = 0;

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
        RefreshEquipmentAnimator();
    }

    private void Update()
    {
        HandleNumberKeys();

        // Slot contents can change any frame (drag/drop, crafting, eating
        // consuming a stack to 0) - re-check the equipped item every frame,
        // same as before.
        RefreshEquipmentAnimator();
    }

    private void HandleNumberKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectSlot(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SelectSlot(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SelectSlot(7);
    }

    private void SelectSlot(int index)
    {
        if (slots == null || index < 0 || index >= slots.Length)
            return;

        selectedSlot = index;

        if (slotVisuals != null)
        {
            for (int i = 0; i < slotVisuals.Length; i++)
            {
                if (slotVisuals[i] != null)
                {
                    slotVisuals[i].SetSelected(i == selectedSlot);
                }
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

    private void RefreshEquipmentAnimator()
    {
        if (playerAnimator == null || defaultPlayerController == null)
            return;

        // Default = player is holding nothing.
        RuntimeAnimatorController targetController = defaultPlayerController;

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

    /// <summary>
    /// Reads whatever InventoryItem GameObject is actually sitting in the
    /// selected Slot right now - no more hardcoded per-slot ItemType.
    /// </summary>
    public bool TryGetSelectedItem(out ItemType itemType)
    {
        itemType = default;

        if (slots == null || selectedSlot < 0 || selectedSlot >= slots.Length)
            return false;

        Slot slot = slots[selectedSlot];
        if (slot == null || slot.currentItem == null)
            return false;

        InventoryItem item = slot.currentItem.GetComponent<InventoryItem>();
        if (item == null)
            return false;

        itemType = item.itemType;
        return true;
    }

    public int GetSelectedSlot()
    {
        return selectedSlot;
    }
}