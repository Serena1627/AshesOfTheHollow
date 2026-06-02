using System.Collections.Generic;
using UnityEngine;

public class BattleSceneManager : MonoBehaviour
{
    [Header("Backgrounds")]
    [SerializeField] private GameObject plainsBackground;
    [SerializeField] private GameObject forestBackground;
    [SerializeField] private GameObject ruinsBackground;
    [SerializeField] private GameObject dungeonBackground;
    [SerializeField] private GameObject houseBackground;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Optional Runtime Parents")]
    [SerializeField] private Transform battlePartyParent;
    [SerializeField] private Transform battleEnemiesParent;

    [Header("Direct BattleScene Testing Only")]
    [Tooltip("Used only when pressing Play directly inside BattleScene.")]
    [SerializeField] private List<GameObject> testPartyPrefabs = new List<GameObject>();

    [Tooltip("Used only when pressing Play directly inside BattleScene.")]
    [SerializeField] private List<GameObject> testEnemyPrefabs = new List<GameObject>();

    private void Awake()
    {
        SetBackground();
        SpawnParty();
        SpawnEnemies();
    }

    private void SetBackground()
    {
        SetBackgroundActive(plainsBackground, false);
        SetBackgroundActive(forestBackground, false);
        SetBackgroundActive(ruinsBackground, false);
        SetBackgroundActive(dungeonBackground, false);
        SetBackgroundActive(houseBackground, false);

        switch (BattleData.backgroundType)
        {
            case BattleBackgroundType.Plains:
                SetBackgroundActive(plainsBackground, true);
                break;

            case BattleBackgroundType.Forest:
                SetBackgroundActive(forestBackground, true);
                break;

            case BattleBackgroundType.Ruins:
                SetBackgroundActive(ruinsBackground, true);
                break;

            case BattleBackgroundType.Dungeon:
                SetBackgroundActive(dungeonBackground, true);
                break;

            case BattleBackgroundType.House:
                SetBackgroundActive(houseBackground, true);
                break;

            default:
                SetBackgroundActive(plainsBackground, true);
                break;
        }
    }

    private void SetBackgroundActive(GameObject background, bool active)
    {
        if (background != null)
        {
            background.SetActive(active);
        }
    }

    private void SpawnParty()
    {
        List<GameObject> prefabsToSpawn = BattleData.partyPrefabs;

        if (prefabsToSpawn == null || prefabsToSpawn.Count == 0)
        {
            prefabsToSpawn = testPartyPrefabs;
            Debug.Log("Using test party prefabs because no BattleData party was supplied.");
        }

        SpawnList(
            prefabsToSpawn,
            partySpawnPoints,
            battlePartyParent,
            "party member"
        );
    }

    private void SpawnEnemies()
    {
        List<GameObject> prefabsToSpawn = BattleData.enemyPrefabs;

        if (prefabsToSpawn == null || prefabsToSpawn.Count == 0)
        {
            prefabsToSpawn = testEnemyPrefabs;
            Debug.Log("Using test enemy prefabs because no BattleData enemies were supplied.");
        }

        SpawnList(
            prefabsToSpawn,
            enemySpawnPoints,
            battleEnemiesParent,
            "enemy"
        );
    }

    private void SpawnList(
        List<GameObject> prefabs,
        Transform[] spawnPoints,
        Transform parent,
        string participantType
    )
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogError("No " + participantType + " prefabs were available to spawn.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned for " + participantType + "s.");
            return;
        }

        int count = Mathf.Min(prefabs.Count, spawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabs[i];
            Transform spawnPoint = spawnPoints[i];

            if (prefab == null)
            {
                Debug.LogWarning("Missing " + participantType + " prefab at index " + i + ".");
                continue;
            }

            if (spawnPoint == null)
            {
                Debug.LogWarning("Missing " + participantType + " spawn point at index " + i + ".");
                continue;
            }

            GameObject spawnedObject = Instantiate(
                prefab,
                spawnPoint.position,
                spawnPoint.rotation,
                parent
            );

            Debug.Log("Spawned " + participantType + ": " + spawnedObject.name);
        }

        if (prefabs.Count > spawnPoints.Length)
        {
            Debug.LogWarning(
                "There are more " + participantType +
                " prefabs than spawn points. Some units were not spawned."
            );
        }
    }
}