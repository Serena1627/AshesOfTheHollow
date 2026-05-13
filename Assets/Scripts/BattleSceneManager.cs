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

    private void Start()
    {
        SetBackground();
        SpawnParty();
        SpawnEnemies();
    }

    private void SetBackground()
    {
        plainsBackground.SetActive(false);
        forestBackground.SetActive(false);
        ruinsBackground.SetActive(false);
        dungeonBackground.SetActive(false);
        houseBackground.SetActive(false);

        switch (BattleData.backgroundType)
        {
            case BattleBackgroundType.Plains:
                plainsBackground.SetActive(true);
                break;
            case BattleBackgroundType.Forest:
                forestBackground.SetActive(true);
                break;
            case BattleBackgroundType.Ruins:
                ruinsBackground.SetActive(true);
                break;
            case BattleBackgroundType.Dungeon:
                dungeonBackground.SetActive(true);
                break;
            case BattleBackgroundType.House:
                houseBackground.SetActive(true);
                break;
        }
    }

    private void SpawnParty()
    {
        SpawnList(BattleData.partyPrefabs, partySpawnPoints);
    }

    private void SpawnEnemies()
    {
        SpawnList(BattleData.enemyPrefabs, enemySpawnPoints);
    }

    private void SpawnList(List<GameObject> prefabs, Transform[] spawnPoints)
    {
        int count = Mathf.Min(prefabs.Count, spawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            Instantiate(prefabs[i], spawnPoints[i].position, Quaternion.identity);
        }
    }
}