using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Quest dashboard: shows one row per active quest with a title and
/// live progress (e.g. "Wolf Hunt  3/10"). Polls QuestManager, so no
/// direct references to individual NPCs are needed.
///
/// Setup:
/// 1. Create a UI Panel positioned under your HP/hunger bars (top-left
///    of the screen), sized to hold a handful of rows.
/// 2. Create ONE row prefab: a small horizontal layout with two
///    TextMeshProUGUI children (title, progress). Save it as a prefab.
/// 3. Add this script to the panel, assign rowPrefab and rowContainer
///    (rowContainer can just be the panel itself, or a child with a
///    Vertical Layout Group so rows stack automatically).
/// 4. Give each QuestGiver a Quest Title in the Inspector - that's
///    what shows here.
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    [Header("References")]
    public GameObject rowPrefab;
    [Tooltip("Parent transform rows get instantiated under. Add a Vertical Layout Group to it so rows stack cleanly.")]
    public RectTransform rowContainer;

    [Header("Behavior")]
    [Tooltip("How often (seconds) to refresh progress text. Doesn't need to be every frame.")]
    public float refreshInterval = 0.25f;
    [Tooltip("Seconds a completed quest's row stays visible (showing full progress) before disappearing from the tracker.")]
    public float completedRowLingerTime = 3f;

    private readonly Dictionary<QuestGiver, RowHandle> rows = new Dictionary<QuestGiver, RowHandle>();
    private float refreshTimer;

    private class RowHandle
    {
        public GameObject rowObject;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI progressText;
        public float completedAt = -1f;
    }

    private void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f) return;
        refreshTimer = refreshInterval;

        if (QuestManager.Instance == null) return;

        var active = QuestManager.Instance.GetActiveQuests();

        // Add rows for any quest we haven't seen yet.
        foreach (QuestGiver quest in active)
        {
            if (!rows.ContainsKey(quest))
            {
                rows[quest] = CreateRow();
            }
        }

        // Update existing rows; remove ones whose quest disappeared or
        // whose "completed" linger time has elapsed.
        List<QuestGiver> toRemove = null;
        foreach (var kvp in rows)
        {
            QuestGiver quest = kvp.Key;
            RowHandle row = kvp.Value;

            bool stillActive = false;
            if (quest != null)
            {
                for (int i = 0; i < active.Count; i++)
                {
                    if (active[i] == quest) { stillActive = true; break; }
                }
            }

            if (!stillActive)
            {
                Destroy(row.rowObject);
                (toRemove ??= new List<QuestGiver>()).Add(quest);
                continue;
            }

            QuestGiver.Snapshot snap = quest.GetSnapshot();

            if (row.titleText != null) row.titleText.text = snap.title;
            if (row.progressText != null)
            {
                row.progressText.text = snap.completed
                    ? "Complete!"
                    : $"{snap.current}/{snap.required}";
            }

            if (snap.completed)
            {
                if (row.completedAt < 0f) row.completedAt = Time.time;
                if (Time.time - row.completedAt >= completedRowLingerTime)
                {
                    Destroy(row.rowObject);
                    (toRemove ??= new List<QuestGiver>()).Add(quest);
                }
            }
        }

        if (toRemove != null)
        {
            foreach (QuestGiver q in toRemove) rows.Remove(q);
        }
    }

    private RowHandle CreateRow()
    {
        GameObject rowObj = Instantiate(rowPrefab, rowContainer != null ? rowContainer : transform);

        RowHandle handle = new RowHandle { rowObject = rowObj };

        // Expects the row prefab to have exactly two TextMeshProUGUI
        // children, in order: [0] title, [1] progress.
        TextMeshProUGUI[] texts = rowObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0) handle.titleText = texts[0];
        if (texts.Length > 1) handle.progressText = texts[1];

        return handle;
    }
}