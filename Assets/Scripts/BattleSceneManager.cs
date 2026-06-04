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

        SpawnList(
            partyToSpawn,
            partySpawnPoints,
            partyParent,
            "party member"
        );

        SpawnList(
            enemiesToSpawn,
            enemySpawnPoints,
            enemyParent,
            "enemy"
        );
    }

    private List<GameObject> GetPartyPrefabs()
    {
        if (BattleData.partyPrefabs != null &&
            BattleData.partyPrefabs.Count > 0)
        {
            return new List<GameObject>(BattleData.partyPrefabs);
        }

        return new List<GameObject>(testPartyPrefabs);
    }

    private List<GameObject> GetEnemyPrefabs()
    {
        if (BattleData.enemyPrefabs != null &&
            BattleData.enemyPrefabs.Count > 0)
        {
            return new List<GameObject>(BattleData.enemyPrefabs);
        }

        return new List<GameObject>(testEnemyPrefabs);
    }

    private void SetBackground()
    {
        SetActive(plainsBackground, false);
        SetActive(forestBackground, false);
        SetActive(ruinsBackground, false);
        SetActive(dungeonBackground, false);
        SetActive(houseBackground, false);

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
                spawned.name
            );
        }

        if (prefabs.Count > spawnPoints.Length)
        {
            Debug.LogWarning(
                "More " +
                participantType +
                " prefabs were supplied than there are spawn points."
            );
        }
    }
}