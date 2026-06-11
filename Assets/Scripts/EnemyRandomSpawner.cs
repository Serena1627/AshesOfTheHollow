using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyRandomSpawner2D : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnEntry
    {
        [Tooltip("The enemy prefab that appears and moves around on the overworld map.")]
        public GameObject overworldPrefab;

        [Tooltip("The enemy prefab used inside the BattleScene. This prefab must have EnemyBattle.")]
        public GameObject battlePrefab;
    }

    [Header("Enemy Spawn Entries")]
    [SerializeField] private List<EnemySpawnEntry> enemySpawnEntries = new List<EnemySpawnEntry>();

    [Header("Spawn Count")]
    [SerializeField] private int numberOfEnemiesToSpawn = 3;

    [Header("Map References")]
    [Tooltip("The visible map background. Used to know the rectangular area enemies can spawn inside.")]
    [SerializeField] private SpriteRenderer mapBackgroundRenderer;

    [Tooltip("Tilemap containing blocked tiles: trees, walls, rocks, ruins, borders, etc.")]
    [SerializeField] private Tilemap blockedObjectTilemap;

    [Tooltip("Optional. Tilemap containing endpoint/exit tiles. Enemies will not spawn there.")]
    [SerializeField] private Tilemap endpointTilemap;

    [Header("Spawn Safety")]
    [Tooltip("How many random attempts the spawner gets per enemy before giving up.")]
    [SerializeField] private int maxAttemptsPerEnemy = 100;

    [Tooltip("Enemies cannot spawn this close to the player.")]
    [SerializeField] private Transform player;

    [SerializeField] private float minimumDistanceFromPlayer = 4f;

    [Tooltip("Enemies cannot spawn this close to each other.")]
    [SerializeField] private float minimumDistanceBetweenEnemies = 2f;

    [Tooltip("Keeps enemies slightly away from the hard map edge.")]
    [SerializeField] private float edgePadding = 1f;

    [Header("Optional Spawn Parent")]
    [SerializeField] private Transform enemyParent;

    [Header("Battle Encounter Settings")]
    [SerializeField] private string battleBackground = "Plains";
    [SerializeField] private string battleSceneName = "BattleScene";

    private readonly List<Vector2> spawnedEnemyPositions = new List<Vector2>();

    private void Start()
    {
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (!CanSpawn())
            return;

        spawnedEnemyPositions.Clear();

        int spawnedCount = 0;

        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            bool spawned = TrySpawnOneEnemy();

            if (spawned)
                spawnedCount++;
        }

        Debug.Log("EnemyRandomSpawner2D: Spawned " + spawnedCount + " enemies.");
    }

    private bool CanSpawn()
    {
        if (enemySpawnEntries == null || enemySpawnEntries.Count == 0)
        {
            Debug.LogWarning("EnemyRandomSpawner2D: No enemy spawn entries assigned.");
            return false;
        }

        foreach (EnemySpawnEntry entry in enemySpawnEntries)
        {
            if (entry == null)
            {
                Debug.LogWarning("EnemyRandomSpawner2D: One enemy spawn entry is null.");
                return false;
            }

            if (entry.overworldPrefab == null)
            {
                Debug.LogWarning("EnemyRandomSpawner2D: One enemy spawn entry is missing an overworld prefab.");
                return false;
            }

            if (entry.battlePrefab == null)
            {
                Debug.LogWarning("EnemyRandomSpawner2D: One enemy spawn entry is missing a battle prefab.");
                return false;
            }

            if (entry.battlePrefab.GetComponent<EnemyBattle>() == null)
            {
                Debug.LogWarning(
                    "EnemyRandomSpawner2D: Battle prefab " +
                    entry.battlePrefab.name +
                    " does not have EnemyBattle."
                );

                return false;
            }
        }

        if (mapBackgroundRenderer == null)
        {
            Debug.LogWarning("EnemyRandomSpawner2D: Map Background Renderer is not assigned.");
            return false;
        }

        if (blockedObjectTilemap == null)
        {
            Debug.LogWarning("EnemyRandomSpawner2D: Blocked Object Tilemap is not assigned.");
            return false;
        }

        return true;
    }

    private bool TrySpawnOneEnemy()
    {
        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            Vector2 randomPosition = GetRandomPositionInsideMap();

            if (!IsValidSpawnPosition(randomPosition))
                continue;

            EnemySpawnEntry enemyEntry = GetRandomEnemyEntry();

            if (enemyEntry == null || enemyEntry.overworldPrefab == null || enemyEntry.battlePrefab == null)
                continue;

            GameObject spawnedEnemy = Instantiate(
                enemyEntry.overworldPrefab,
                randomPosition,
                Quaternion.identity,
                enemyParent
            );

            ConfigureSpawnedEnemyEncounter(
                spawnedEnemy,
                enemyEntry.overworldPrefab,
                enemyEntry.battlePrefab
            );

            EnemyChasePlayer2D chaseScript = spawnedEnemy.GetComponent<EnemyChasePlayer2D>();

            if (chaseScript != null)
            {
                chaseScript.SetBlockedObjectTilemap(blockedObjectTilemap);
            }

            spawnedEnemyPositions.Add(randomPosition);

            return true;
        }

        Debug.LogWarning("EnemyRandomSpawner2D: Failed to find a valid spawn position.");
        return false;
    }

    private void ConfigureSpawnedEnemyEncounter(
        GameObject spawnedEnemy,
        GameObject overworldPrefab,
        GameObject battlePrefab
    )
    {
        BattleEncounterTrigger encounterTrigger =
            spawnedEnemy.GetComponent<BattleEncounterTrigger>();

        if (encounterTrigger == null)
        {
            Debug.LogWarning(spawnedEnemy.name + " does not have BattleEncounterTrigger.");
            return;
        }

        string encounterId =
            overworldPrefab.name + "_" + spawnedEnemyPositions.Count.ToString("00");

        List<GameObject> encounterEnemies = new List<GameObject>
        {
            battlePrefab
        };

        encounterTrigger.ConfigureEncounter(
            encounterId,
            spawnedEnemy,
            battleBackground,
            encounterEnemies,
            battleSceneName
        );

        Debug.Log(
            "Configured encounter " +
            encounterId +
            " | Overworld prefab: " +
            overworldPrefab.name +
            " | Battle prefab: " +
            battlePrefab.name
        );
    }

    private Vector2 GetRandomPositionInsideMap()
    {
        Bounds bounds = mapBackgroundRenderer.bounds;

        float minX = bounds.min.x + edgePadding;
        float maxX = bounds.max.x - edgePadding;
        float minY = bounds.min.y + edgePadding;
        float maxY = bounds.max.y - edgePadding;

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        return new Vector2(randomX, randomY);
    }

    private bool IsValidSpawnPosition(Vector2 worldPosition)
    {
        if (IsBlocked(worldPosition))
            return false;

        if (IsEndpoint(worldPosition))
            return false;

        if (IsTooCloseToPlayer(worldPosition))
            return false;

        if (IsTooCloseToOtherEnemies(worldPosition))
            return false;

        return true;
    }

    private bool IsBlocked(Vector2 worldPosition)
    {
        Vector3Int cellPosition = blockedObjectTilemap.WorldToCell(worldPosition);

        return blockedObjectTilemap.HasTile(cellPosition);
    }

    private bool IsEndpoint(Vector2 worldPosition)
    {
        if (endpointTilemap == null)
            return false;

        Vector3Int cellPosition = endpointTilemap.WorldToCell(worldPosition);

        return endpointTilemap.HasTile(cellPosition);
    }

    private bool IsTooCloseToPlayer(Vector2 worldPosition)
    {
        if (player == null)
            return false;

        float distance = Vector2.Distance(worldPosition, player.position);

        return distance < minimumDistanceFromPlayer;
    }

    private bool IsTooCloseToOtherEnemies(Vector2 worldPosition)
    {
        foreach (Vector2 enemyPosition in spawnedEnemyPositions)
        {
            float distance = Vector2.Distance(worldPosition, enemyPosition);

            if (distance < minimumDistanceBetweenEnemies)
                return true;
        }

        return false;
    }

    private EnemySpawnEntry GetRandomEnemyEntry()
    {
        if (enemySpawnEntries == null || enemySpawnEntries.Count == 0)
            return null;

        int randomIndex = Random.Range(0, enemySpawnEntries.Count);

        return enemySpawnEntries[randomIndex];
    }
}