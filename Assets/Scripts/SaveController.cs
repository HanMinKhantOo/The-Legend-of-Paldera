using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Save Settings")]
    [SerializeField] private string saveFileName = "saveData.json";

    private string SavePath
    {
        get
        {
            return Path.Combine(
                Application.persistentDataPath,
                saveFileName
            );
        }
    }

    private void Start()
    {
        LoadGame();
    }

    public void SaveGame()
    {
        if (player == null)
        {
            Debug.LogError("SaveController: Player has not been assigned!");
            return;
        }

        SaveData data = new SaveData();

        data.playerPosition = player.position;

        // Your current game does not use the tutorial's
        // mapBoundary system, so leave this empty for now.
        data.mapBoundary = "";

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(SavePath, json);

        Debug.Log("Game saved successfully!");
        Debug.Log("Save location: " + SavePath);
    }

    public void LoadGame()
    {
        if (player == null)
        {
            Debug.LogError("SaveController: Player has not been assigned!");
            return;
        }

        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file exists yet. Starting normally.");
            return;
        }

        string json = File.ReadAllText(SavePath);

        SaveData data = JsonUtility.FromJson<SaveData>(json);

        player.position = data.playerPosition;

        Debug.Log("Game loaded successfully!");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted.");
        }
    }
}