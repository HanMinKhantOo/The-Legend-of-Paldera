
using UnityEngine;

public class StoneHealth : MonoBehaviour
{
    [Header("Mining Settings")]
    [SerializeField] private int maxHits = 8;

    [Header("Stone Drops")]
    [SerializeField] private GameObject stonePickupPrefab;
    [SerializeField] private int stoneDrops = 5;
    [SerializeField] private float dropRadius = 0.7f;

    private int currentHits = 0;
    private bool isBroken = false;

    public void TakeHit(int damage = 1)
    {
        if (isBroken) return;

        currentHits += damage;

        Debug.Log(gameObject.name + " hit: "
            + currentHits + "/" + maxHits);

        if (currentHits >= maxHits)
        {
            BreakStone();
        }
    }

    private void BreakStone()
    {
        if (isBroken) return;

        if (stonePickupPrefab == null)
        {
            Debug.LogError("Stone pickup prefab missing!");
            return;
        }

        isBroken = true;

        for (int i = 0; i < stoneDrops; i++)
        {
            Vector2 offset =
                Random.insideUnitCircle * dropRadius;

            Instantiate(
                stonePickupPrefab,
                transform.position + (Vector3)offset,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}