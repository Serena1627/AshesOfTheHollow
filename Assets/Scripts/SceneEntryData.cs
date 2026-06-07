public static class SceneEntryData
{
    private static string destinationSceneName;
    private static string destinationSpawnId;

    public static void SetNextEntry(string sceneName, string spawnId)
    {
        destinationSceneName = sceneName;
        destinationSpawnId = spawnId;

        UnityEngine.Debug.Log(
            "SceneEntryData set: " +
            destinationSceneName +
            " / " +
            destinationSpawnId
        );
    }

    public static bool HasPendingEntryForScene(string currentSceneName)
    {
        if (string.IsNullOrWhiteSpace(destinationSceneName) ||
            string.IsNullOrWhiteSpace(destinationSpawnId))
        {
            return false;
        }

        return destinationSceneName == currentSceneName;
    }

    public static bool TryConsumeEntryForScene(
        string currentSceneName,
        out string spawnId
    )
    {
        spawnId = null;

        UnityEngine.Debug.Log(
            "SceneEntryData checked by: " +
            currentSceneName +
            " | stored: " +
            destinationSceneName +
            " / " +
            destinationSpawnId
        );

        if (string.IsNullOrWhiteSpace(destinationSceneName) ||
            string.IsNullOrWhiteSpace(destinationSpawnId))
        {
            return false;
        }

        if (destinationSceneName != currentSceneName)
        {
            return false;
        }

        spawnId = destinationSpawnId;

        destinationSceneName = null;
        destinationSpawnId = null;

        return true;
    }
}