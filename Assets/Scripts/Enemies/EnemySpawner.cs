using System.Collections;
using UnityEngine;

/// <summary>
/// Place one of these at each designated spawn location in the scene and
/// assign the Wolf or Boar prefab. Spawns one enemy at Start, and respawns
/// it at this same location after respawnDelay once it dies - the enemy
/// instance is reused (EnemyHealth.ResetHealth + EnemyAI.Initialize) rather
/// than destroyed/reinstantiated, so no pooling system was needed.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public GameObject enemyPrefab;
    [Tooltip("Seconds after death before the mob reappears here.")]
    public float respawnDelay = 30f;
    [Tooltip("If true, uses this spawner's own position. Otherwise assign spawnPointOverride.")]
    public bool useOwnTransform = true;
    public Transform spawnPointOverride;

    private GameObject currentInstance;
    private EnemyHealth currentHealth;
    private EnemyAI currentAI;

    private Vector3 SpawnPosition =>
        useOwnTransform || spawnPointOverride == null
            ? transform.position
            : spawnPointOverride.position;

    private void Start()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] No enemyPrefab assigned on " + gameObject.name);
            return;
        }

        currentInstance = Instantiate(enemyPrefab, SpawnPosition, Quaternion.identity);
        currentHealth = currentInstance.GetComponent<EnemyHealth>();
        currentAI = currentInstance.GetComponent<EnemyAI>();

        if (currentAI != null)
        {
            currentAI.Initialize(SpawnPosition);
        }

        if (currentHealth != null)
        {
            currentHealth.OnDeath += HandleEnemyDeath;
        }
        else
        {
            Debug.LogError("[EnemySpawner] " + enemyPrefab.name + " has no EnemyHealth component.");
        }
    }

    private void HandleEnemyDeath(EnemyHealth deadHealth)
    {
        deadHealth.OnDeath -= HandleEnemyDeath;
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (currentInstance == null)
        {
            // Instance was destroyed some other way - spawn a fresh one.
            SpawnEnemy();
            yield break;
        }

        currentHealth.ResetHealth();
        currentAI.Initialize(SpawnPosition);
        currentHealth.OnDeath += HandleEnemyDeath;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(SpawnPosition, Vector3.one * 0.5f);
    }
}
