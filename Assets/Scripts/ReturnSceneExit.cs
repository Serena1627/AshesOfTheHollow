using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnSceneExit : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    [Tooltip("Spawn point ID to use in the destination scene.")]
    [SerializeField] private string destinationSpawnId;

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

        if (string.IsNullOrWhiteSpace(destinationSpawnId))
        {
            Debug.LogWarning(
                "ReturnSceneExit: Destination Spawn Id is empty. " +
                "The destination scene will use its default player position."
            );
        }

        isLoading = true;

        SceneEntryData.SetNextEntry(
            nextSceneName,
            destinationSpawnId
        );

        Debug.Log(
            "Loading " +
            nextSceneName +
            " at spawn point: " +
            destinationSpawnId
        );

        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}