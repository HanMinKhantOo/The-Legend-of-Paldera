using UnityEngine;

public class TreeHealth : MonoBehaviour
{
    [Header("Tree Health")]
    [SerializeField] private int maxHits = 8;

    [Header("Log Drops")]
    [SerializeField] private GameObject logPickupPrefab;
    [SerializeField] private int minLogDrops = 3;
    [SerializeField] private int maxLogDrops = 5;
    [SerializeField] private float dropRadius = 0.7f;

    private int currentHits = 0;
    private bool isBroken = false;

    public void TakeHit(int damage = 1)
    {
        if (isBroken)
            return;

        currentHits += damage;

        Debug.Log(
            gameObject.name + " hit: " +
            currentHits + "/" + maxHits
        );

        if (currentHits >= maxHits)
        {
            BreakTree();
        }
    }

    private void BreakTree()
    {
        if (isBroken)
            return;

        isBroken = true;

        // Spawn the logs BEFORE removing the tree.
        DropLogs();

        Debug.Log(gameObject.name + " has been broken!");

        Destroy(gameObject);
    }

    private void DropLogs()
    {
        if (logPickupPrefab == null)
        {
            Debug.LogError(
                "No Object_Log prefab assigned on " + gameObject.name
            );

            return;
        }

        int numberOfLogs =
            Random.Range(minLogDrops, maxLogDrops + 1);

        Debug.Log(
            gameObject.name +
            " dropped " +
            numberOfLogs +
            " logs."
        );

        for (int i = 0; i < numberOfLogs; i++)
        {
            Vector2 randomOffset =
                Random.insideUnitCircle * dropRadius;

            Vector3 spawnPosition =
                transform.position +
                new Vector3(
                    randomOffset.x,
                    randomOffset.y,
                    0f
                );

            Instantiate(
                logPickupPrefab,
                spawnPosition,
                Quaternion.identity
            );
        }
    }
}