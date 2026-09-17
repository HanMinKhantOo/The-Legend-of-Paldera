using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject selection;

    private void Awake()
    {
        FindReferences();
    }

    private void OnValidate()
    {
        FindReferences();
    }

    private void FindReferences()
    {
        if (itemIcon == null)
        {
            Transform iconTransform = transform.Find("Icon");

            if (iconTransform != null)
                itemIcon = iconTransform.GetComponent<Image>();
        }

        if (quantityText == null)
        {
            Transform quantityTransform = transform.Find("QuantityText");

            if (quantityTransform != null)
                quantityText =
                    quantityTransform.GetComponent<TextMeshProUGUI>();
        }

        if (selection == null)
        {
            Transform selectionTransform = transform.Find("Selection");

            if (selectionTransform != null)
                selection = selectionTransform.gameObject;
        }
    }

    public void SetItem(Sprite icon, int quantity)
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
        }

        if (quantityText != null)
        {
            if (quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.text = "";
                quantityText.gameObject.SetActive(false);
            }
        }
    }

    public void ClearSlot()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }
    }

    public void SetSelected(bool selected)
    {
        // Never show the separate Selection image.
        if (selection != null)
        {
            selection.SetActive(false);
        }

        // Tint the actual slot instead.
        Image slotImage = GetComponent<Image>();

        if (slotImage != null)
        {
            slotImage.color = selected
                ? new Color(1f, 0.85f, 0.45f, 1f)
                : Color.white;
        }
    }
}