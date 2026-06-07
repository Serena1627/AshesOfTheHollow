public static class SceneEntryData
{
    private static string destinationSceneName;
    private static string destinationSpawnId;

    public static void SetNextEntry(string sceneName, string spawnId)
    {
        destinationSceneName = sceneName;
        destinationSpawnId = spawnId;
    }

    public static bool TryConsumeEntryForScene(
        string currentSceneName,
        out string spawnId
    )
    {
        spawnId = null;

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