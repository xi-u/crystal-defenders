using UnityEngine;

/// A map location that spawns enemies. It does not decide how many or how
/// often. The WaveDirector calls Spawn(). Keeping the "when" out of here is
/// what makes the WaveDirector the single place game flow is controlled.
public class SpawnZone : MonoBehaviour
{
    [SerializeField] Enemy enemyPrefab;

    [Tooltip("Enemies appear at a random point inside this radius, so a wave " +
             "does not walk to the crystal as one perfectly overlapping column.")]
    [SerializeField] float spawnRadius = 1.5f;

    public Enemy Spawn(Crystal target)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"{name} has no enemy prefab assigned.", this);
            return null;
        }

        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 position = new Vector3(
            transform.position.x + offset.x,
            enemyPrefab.transform.position.y,   // keep the prefab's authored height
            transform.position.z + offset.y);

        Enemy enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.Initialise(target);
        return enemy;
    }

    // Draws the spawn radius in the Scene view so the tuning is visible
    // without entering play mode.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
