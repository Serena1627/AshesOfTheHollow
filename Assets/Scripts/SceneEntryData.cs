public static class SceneEntryData
{
    private static string destinationSceneName;
    private static string destinationSpawnId;
    private static string destinationFacing;

    public static void SetNextEntry(
        string sceneName,
        string spawnId,
        string facing = ""
    )
    {
        destinationSceneName = sceneName;
        destinationSpawnId = spawnId;
        destinationFacing = facing;

        UnityEngine.Debug.Log(
            "SceneEntryData set: " +
            destinationSceneName +
            " / " +
            destinationSpawnId +
            " / facing: " +
            destinationFacing
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
        out string spawnId,
        out string facing
    )
    {
        spawnId = null;
        facing = null;

        UnityEngine.Debug.Log(
            "SceneEntryData checked by: " +
            currentSceneName +
            " | stored: " +
            destinationSceneName +
            " / " +
            destinationSpawnId +
            " / facing: " +
            destinationFacing
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
        facing = destinationFacing;

        destinationSceneName = null;
        destinationSpawnId = null;
        destinationFacing = null;

        return true;
    }
}