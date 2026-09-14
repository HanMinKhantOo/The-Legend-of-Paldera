using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CraftingInventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;

    [Header("Crafting")]
    [SerializeField] private bool selectableForCrafting = false;

    public Sprite CurrentIcon { get; private set; }
    public ItemType CurrentItemType { get; private set; }
    public int CurrentQuantity { get; private set; }
    public bool HasItem { get; private set; }

    public static Sprite SelectedIcon { get; private set; }
    public static ItemType SelectedItemType { get; private set; }
    public static int SelectedQuantity { get; private set; }
    public static bool HasSelection { get; private set; }

    public void SetItem(Sprite icon, int quantity, ItemType itemType)
    {
        CurrentIcon = icon;
        CurrentItemType = itemType;
        CurrentQuantity = quantity;
        HasItem = icon != null;

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
            itemIcon.preserveAspect = true;
        }

        if (quantityText != null)
        {
            quantityText.text = quantity > 0
                ? quantity.ToString()
                : "";
        }
    }

    public void ClearSlot()
    {
        CurrentIcon = null;
        CurrentQuantity = 0;
        HasItem = false;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!selectableForCrafting || !HasItem)
            return;

        SelectedIcon = CurrentIcon;
        SelectedItemType = CurrentItemType;
        SelectedQuantity = CurrentQuantity;
        HasSelection = true;

        Debug.Log("Selected crafting material: " + CurrentItemType);
    }
}