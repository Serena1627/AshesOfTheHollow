using UnityEngine;
using UnityEngine.SceneManagement;

public class StairSceneExit : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "DamagedHouseScene";

    [Header("Optional Requirement")]
    [SerializeField] private bool requireHolySword = false;
    [SerializeField] private BasementHolyChest holyChest;

    private bool isLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || !other.CompareTag("Player"))
        {
            return;
        }

        if (requireHolySword)
        {
            if (holyChest == null)
            {
                Debug.LogWarning("Stair exit requires the holy sword, but no chest reference is assigned.");
                return;
            }

            if (!holyChest.opened)
            {
                Debug.Log("Kael should find a way to protect Mira before leaving.");
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("Next Scene Name is empty on StairSceneExit.");
            return;
        }

        isLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }
}