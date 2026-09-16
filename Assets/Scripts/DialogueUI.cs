using UnityEngine;
using TMPro;

/// <summary>
/// Simple single-line dialogue popup. One instance lives in the scene
/// (on a Canvas or the DialoguePanel itself), and any NPC calls
/// DialogueUI.Instance.Show(...) to display its line. Press E again
/// (or click) to close.
///
/// Setup:
/// 1. Create a UI Canvas if you don't have one.
/// 2. Under it, create a Panel ("DialoguePanel") positioned near the
///    bottom of the screen, with a background Image and two child
///    TextMeshProUGUI elements - one for the speaker name, one for
///    the line itself. Style the panel/fonts however fits your game.
/// 3. Add this script to DialoguePanel (or the Canvas) and drag
///    DialoguePanel / nameText / lineText into the fields below.
/// 4. Disable DialoguePanel by default in the Hierarchy - this script
///    turns it on/off at runtime.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI lineText;

    public bool IsOpen => dialoguePanel != null && dialoguePanel.activeSelf;

    private void Awake()
    {
        Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void Show(string speakerName, string line)
    {
        if (dialoguePanel == null) return;

        if (speakerNameText != null) speakerNameText.text = speakerName;
        if (lineText != null) lineText.text = line;
        dialoguePanel.SetActive(true);
    }

    public void Close()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}