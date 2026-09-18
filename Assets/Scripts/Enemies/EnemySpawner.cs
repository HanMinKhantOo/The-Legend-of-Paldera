using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place one of these at each designated spawn location in the scene and
/// assign the Wolf or Boar prefab. Spawns `enemyCount` enemies at Start,
/// scattered a little around this point so they don't stack exactly on top
/// of each other, and respawns each one independently at its own original
/// position after respawnDelay once it dies - each enemy instance is reused
/// (EnemyHealth.ResetHealth + EnemyAI.Initialize) rather than destroyed and
/// reinstantiated, so no pooling system was needed.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public GameObject enemyPrefab;
    [Tooltip("How many of this enemy this spawner keeps alive at this location.")]
    [Range(1, 10)]
    public int enemyCount = 3;
    [Tooltip("Random scatter radius around this spawner's position so multiple enemies don't spawn stacked on top of each other.")]
    public float spawnScatterRadius = 1.5f;
    [Tooltip("Seconds after death before that specific mob reappears at its own spot.")]
    public float respawnDelay = 30f;
    [Tooltip("If true, uses this spawner's own position as the center. Otherwise assign spawnPointOverride.")]
    public bool useOwnTransform = true;
    public Transform spawnPointOverride;

    private class Slot
    {
        public Vector3 spawnPosition;
        public GameObject instance;
        public EnemyHealth health;
        public EnemyAI ai;
    }

    private readonly List<Slot> slots = new List<Slot>();

    private Vector3 CenterPosition =>
        useOwnTransform || spawnPointOverride == null
            ? transform.position
            : spawnPointOverride.position;

    private void Start()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnScatterRadius;
            Vector3 slotPosition = CenterPosition + new Vector3(offset.x, offset.y, 0f);

            var slot = new Slot { spawnPosition = slotPosition };
            slots.Add(slot);

            SpawnEnemy(slot);
        }
    }

    private void SpawnEnemy(Slot slot)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] No enemyPrefab assigned on " + gameObject.name);
            return;
        }

        slot.instance = Instantiate(enemyPrefab, slot.spawnPosition, Quaternion.identity);
        slot.health = slot.instance.GetComponent<EnemyHealth>();
        slot.ai = slot.instance.GetComponent<EnemyAI>();

        if (slot.ai != null)
        {
            slot.ai.Initialize(slot.spawnPosition);
        }

        if (slot.health != null)
        {
            slot.health.OnDeath += (deadHealth) => HandleEnemyDeath(slot, deadHealth);
        }
        else
        {
            Debug.LogError("[EnemySpawner] " + enemyPrefab.name + " has no EnemyHealth component.");
        }
    }

    private void HandleEnemyDeath(Slot slot, EnemyHealth deadHealth)
    {
        StartCoroutine(RespawnAfterDelay(slot));
    }

    private IEnumerator RespawnAfterDelay(Slot slot)
    {
        yield return new WaitForSeconds(respawnDelay);

        if (slot.instance == null)
        {
            // Instance was destroyed some other way - spawn a fresh one in its slot.
            SpawnEnemy(slot);
            yield break;
        }

        slot.health.ResetHealth();
        slot.ai.Initialize(slot.spawnPosition);
        slot.health.OnDeath += (deadHealth) => HandleEnemyDeath(slot, deadHealth);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(CenterPosition, spawnScatterRadius);
        Gizmos.DrawWireCube(CenterPosition, Vector3.one * 0.3f);
    }
}