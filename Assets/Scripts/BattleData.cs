using System.Collections.Generic;
using UnityEngine;

public static class BattleData
{
    public static string previousSceneName;
    public static BattleBackgroundType backgroundType;

    public static List<GameObject> enemyPrefabs = new List<GameObject>();
    public static List<GameObject> partyPrefabs = new List<GameObject>();

    public static string currentEncounterId;

    // Position where Kael entered the battle.
    public static Vector3 previousPlayerPosition;
    public static bool hasPreviousPlayerPosition;

    private static readonly HashSet<string> defeatedEncounterIds =
        new HashSet<string>();

    public static void SavePlayerPosition(Vector3 position)
    {
        previousPlayerPosition = position;
        hasPreviousPlayerPosition = true;

        Debug.Log("Saved player position before battle: " + position);
    }

    public static bool TryGetReturnPosition(out Vector3 position)
    {
        position = previousPlayerPosition;
        return hasPreviousPlayerPosition;
    }

    public static void ClearReturnPosition()
    {
        hasPreviousPlayerPosition = false;
    }

    public static void MarkCurrentEncounterDefeated()
    {
        if (string.IsNullOrWhiteSpace(currentEncounterId))
        {
            Debug.LogWarning("No current encounter ID was assigned when battle ended.");
            return;
        }

        defeatedEncounterIds.Add(currentEncounterId);
        Debug.Log("Encounter defeated: " + currentEncounterId);
    }

    public static bool IsEncounterDefeated(string encounterId)
    {
        if (string.IsNullOrWhiteSpace(encounterId))
        {
            return false;
        }

        return defeatedEncounterIds.Contains(encounterId);
    }

    public static void ClearCurrentBattleSetup()
    {
        enemyPrefabs.Clear();
        partyPrefabs.Clear();
        currentEncounterId = null;

        // Do not clear previousPlayerPosition here.
        // The overworld scene still needs it after loading.
    }
}