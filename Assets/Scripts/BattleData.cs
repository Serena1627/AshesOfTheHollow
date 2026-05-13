using System.Collections.Generic;
using UnityEngine;

public static class BattleData
{
    public static string previousSceneName;
    public static BattleBackgroundType backgroundType;

    public static List<GameObject> partyPrefabs = new();
    public static List<GameObject> enemyPrefabs = new();
}