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

    [Header("Spawn Point Parents")]
    [Tooltip("Assign the PartySpawnPoints parent object here.")]
    [SerializeField] private Transform partySpawnPointsParent;

    [Tooltip("Assign the EnemySpawnPoints parent object here.")]
    [SerializeField] private Transform enemySpawnPointsParent;

    [Header("Manual Spawn Points")]
    [Tooltip("Optional. If empty or too small, children of PartySpawnPoints Parent will be used.")]
    [SerializeField] private Transform[] partySpawnPoints;

    [Tooltip("Optional. If empty or too small, children of EnemySpawnPoints Parent will be used.")]
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Optional Runtime Parents")]
    [SerializeField] private Transform partyParent;
    [SerializeField] private Transform enemyParent;

    [Header("Direct BattleScene Testing Only")]
    [Tooltip("Used only when pressing Play directly in BattleScene.")]
    [SerializeField] private List<GameObject> testPartyPrefabs =
        new List<GameObject>();

    [Tooltip("Used only when pressing Play directly in BattleScene.")]
    [SerializeField] private List<GameObject> testEnemyPrefabs =
        new List<GameObject>();

    private void Awake()
    {
        SetBackground();

        List<GameObject> partyToSpawn = GetPartyPrefabs();
        List<GameObject> enemiesToSpawn = GetEnemyPrefabs();

        Transform[] finalPartySpawnPoints =
            GetUsableSpawnPoints(
                partySpawnPoints,
                partySpawnPointsParent,
                partyToSpawn.Count,
                "party"
            );

        Transform[] finalEnemySpawnPoints =
            GetUsableSpawnPoints(
                enemySpawnPoints,
                enemySpawnPointsParent,
                enemiesToSpawn.Count,
                "enemy"
            );

        Debug.Log("===== BATTLE SCENE MANAGER SPAWN CHECK =====");
        Debug.Log("Party prefabs supplied: " + partyToSpawn.Count);
        Debug.Log("Party spawn points usable: " + finalPartySpawnPoints.Length);
        Debug.Log("Enemy prefabs supplied: " + enemiesToSpawn.Count);
        Debug.Log("Enemy spawn points usable: " + finalEnemySpawnPoints.Length);

        SpawnList(
            partyToSpawn,
            finalPartySpawnPoints,
            partyParent,
            "party member"
        );

        SpawnList(
            enemiesToSpawn,
            finalEnemySpawnPoints,
            enemyParent,
            "enemy"
        );
    }

    private List<GameObject> GetPartyPrefabs()
    {
        if (BattleData.partyPrefabs != null &&
            BattleData.partyPrefabs.Count > 0)
        {
            Debug.Log("Using BattleData party prefabs.");

            foreach (GameObject prefab in BattleData.partyPrefabs)
            {
                Debug.Log(
                    prefab != null
                        ? "BattleData party prefab: " + prefab.name
                        : "BattleData party prefab: NULL"
                );
            }

            return new List<GameObject>(BattleData.partyPrefabs);
        }

        Debug.LogWarning(
            "BattleData.partyPrefabs was empty. Using test party prefabs."
        );

        return new List<GameObject>(testPartyPrefabs);
    }

    private List<GameObject> GetEnemyPrefabs()
    {
        if (BattleData.enemyPrefabs != null &&
            BattleData.enemyPrefabs.Count > 0)
        {
            Debug.Log("Using BattleData enemy prefabs.");

            foreach (GameObject prefab in BattleData.enemyPrefabs)
            {
                Debug.Log(
                    prefab != null
                        ? "BattleData enemy prefab: " + prefab.name
                        : "BattleData enemy prefab: NULL"
                );
            }

            return new List<GameObject>(BattleData.enemyPrefabs);
        }

        Debug.LogWarning(
            "BattleData.enemyPrefabs was empty. Using test enemy prefabs."
        );

        return new List<GameObject>(testEnemyPrefabs);
    }

    private Transform[] GetUsableSpawnPoints(
        Transform[] manualSpawnPoints,
        Transform spawnParent,
        int requiredCount,
        string spawnType
    )
    {
        List<Transform> usableSpawnPoints = new List<Transform>();

        if (manualSpawnPoints != null)
        {
            foreach (Transform spawnPoint in manualSpawnPoints)
            {
                if (spawnPoint != null)
                {
                    usableSpawnPoints.Add(spawnPoint);
                }
            }
        }

        if (usableSpawnPoints.Count >= requiredCount)
        {
            return usableSpawnPoints.ToArray();
        }

        if (spawnParent == null)
        {
            Debug.LogWarning(
                "No " +
                spawnType +
                " spawn parent assigned, and manual spawn points are not enough."
            );

            return usableSpawnPoints.ToArray();
        }

        usableSpawnPoints.Clear();

        for (int i = 0; i < spawnParent.childCount; i++)
        {
            Transform child = spawnParent.GetChild(i);

            if (child != null)
            {
                usableSpawnPoints.Add(child);
            }
        }

        Debug.Log(
            "Using " +
            usableSpawnPoints.Count +
            " " +
            spawnType +
            " spawn point children from " +
            spawnParent.name +
            "."
        );

        return usableSpawnPoints.ToArray();
    }

    private void SetBackground()
    {
        SetActive(plainsBackground, false);
        SetActive(forestBackground, false);
        SetActive(ruinsBackground, false);
        SetActive(dungeonBackground, false);
        SetActive(houseBackground, false);

        Debug.Log("Battle background selected: " + BattleData.backgroundType);

        switch (BattleData.backgroundType)
        {
            case BattleBackgroundType.Plains:
                SetActive(plainsBackground, true);
                break;

            case BattleBackgroundType.Forest:
                SetActive(forestBackground, true);
                break;

            case BattleBackgroundType.Ruins:
                SetActive(ruinsBackground, true);
                break;

            case BattleBackgroundType.Dungeon:
                SetActive(dungeonBackground, true);
                break;

            case BattleBackgroundType.House:
                SetActive(houseBackground, true);
                break;

            default:
                SetActive(plainsBackground, true);
                break;
        }
    }

    private void SetActive(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
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
            Debug.LogError(
                "No " + participantType + " prefabs were supplied to BattleScene."
            );

            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                "No " + participantType + " spawn points are assigned."
            );

            return;
        }

        int count = Mathf.Min(prefabs.Count, spawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            if (prefabs[i] == null || spawnPoints[i] == null)
            {
                Debug.LogWarning(
                    "Missing " +
                    participantType +
                    " prefab or spawn point at index " +
                    i +
                    "."
                );

                continue;
            }

            GameObject spawned = Instantiate(
                prefabs[i],
                spawnPoints[i].position,
                spawnPoints[i].rotation,
                parent
            );

            Debug.Log(
                "Spawned " +
                participantType +
                ": " +
                spawned.name +
                " at " +
                spawnPoints[i].name
            );
        }

        if (prefabs.Count > spawnPoints.Length)
        {
            Debug.LogWarning(
                "More " +
                participantType +
                " prefabs were supplied than there are spawn points. " +
                "Prefab count: " +
                prefabs.Count +
                " | Spawn point count: " +
                spawnPoints.Length
            );
        }
    }
}