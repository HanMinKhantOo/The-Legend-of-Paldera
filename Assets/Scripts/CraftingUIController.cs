using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingUIController : MonoBehaviour
{
    [Header("Crafting UI")]
    [SerializeField] private GameObject craftingPage;

    private void Start()
    {
        if (craftingPage != null)
        {
            craftingPage.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null || craftingPage == null)
            return;

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            craftingPage.SetActive(!craftingPage.activeSelf);
        }
    }

    public bool IsCraftingOpen()
    {
        return craftingPage != null && craftingPage.activeSelf;
    }
}