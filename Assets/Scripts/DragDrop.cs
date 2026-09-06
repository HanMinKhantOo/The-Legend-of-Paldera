using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private Transform originalParent;
    private Slot originalSlot;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSlot = originalParent.GetComponent<Slot>();

        // Temporarily remove this item from its slot
        if (originalSlot != null)
        {
            originalSlot.currentItem = null;
        }

        // Move item above the inventory UI while dragging
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        // Allows Unity to detect the slot underneath the item
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        Slot targetSlot = FindSlotUnderPointer(eventData);

        if (targetSlot != null)
        {
            // If target slot already contains another item, swap them
            if (targetSlot.currentItem != null)
            {
                GameObject otherItem = targetSlot.currentItem;

                otherItem.transform.SetParent(originalParent);
                otherItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                if (originalSlot != null)
                {
                    originalSlot.currentItem = otherItem;
                }
            }

            // Put dragged item into new slot
            transform.SetParent(targetSlot.transform);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            targetSlot.currentItem = gameObject;
        }
        else
        {
            // Invalid drop: return to original slot
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            if (originalSlot != null)
            {
                originalSlot.currentItem = gameObject;
            }
        }
    }

    private Slot FindSlotUnderPointer(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Slot slot = result.gameObject.GetComponent<Slot>();

            if (slot == null)
            {
                slot = result.gameObject.GetComponentInParent<Slot>();
            }

            if (slot != null)
            {
                return slot;
            }
        }

        return null;
    }
}