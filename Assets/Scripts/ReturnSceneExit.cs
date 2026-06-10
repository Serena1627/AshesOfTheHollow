using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnSceneExit : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    [Tooltip("Spawn point ID to use in the destination scene.")]
    [SerializeField] private string destinationSpawnId;

    [Tooltip("front, back, left, or right")]
    [SerializeField] private string destinationFacing = "back";

    [Header("Optional Requirement")]
    [SerializeField] private bool requirePaladinRecruited;

    private bool isLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || !other.CompareTag("Player"))
        {
            return;
        }

        if (requirePaladinRecruited)
        {
            if (PartyManager.Instance == null ||
                !PartyManager.Instance.PaladinRecruited)
            {
                Debug.Log("Kael cannot return yet.");
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("ReturnSceneExit: Next Scene Name is empty.");
            return;
        }

        isLoading = true;

        SceneEntryData.SetNextEntry(
            nextSceneName,
            destinationSpawnId,
            destinationFacing
        );

        Debug.Log(
            "Loading " +
            nextSceneName +
            " at spawn point: " +
            destinationSpawnId +
            " facing: " +
            destinationFacing
        );

        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}