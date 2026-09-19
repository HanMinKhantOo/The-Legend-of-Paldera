using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry of every QuestGiver currently in the scene. Exists so
/// UI (QuestTrackerUI) can list active quests without needing a direct
/// reference to each NPC. Any QuestGiver registers itself on Awake and
/// unregisters on destroy - no scene wiring required beyond that.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private readonly List<QuestGiver> activeQuests = new List<QuestGiver>();

    private void Awake()
    {
        Instance = this;
    }

    public void Register(QuestGiver quest)
    {
        if (!activeQuests.Contains(quest)) activeQuests.Add(quest);
    }

    public void Unregister(QuestGiver quest)
    {
        activeQuests.Remove(quest);
    }

    public IReadOnlyList<QuestGiver> GetActiveQuests() => activeQuests;
}
