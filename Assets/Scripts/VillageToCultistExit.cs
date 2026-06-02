using UnityEngine;
using UnityEngine.SceneManagement;

public class VillageToCultistExit : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string cultistSceneName = "CultistScene";

    [Header("Story Requirement")]
    [SerializeField] private bool requirePaladinRecruited = true;

    private bool isLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || !other.CompareTag("Player"))
        {
            return;
        }

        if (requirePaladinRecruited)
        {
            if (PartyManager.Instance == null)
            {
                Debug.LogWarning(
                    "Cannot enter the cultist area because PartyManager was not found."
                );
                return;
            }

            if (!PartyManager.Instance.PaladinRecruited)
            {
                Debug.Log(
                    "Kael should speak with the Paladin before continuing."
                );
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(cultistSceneName))
        {
            Debug.LogWarning(
                "Cultist Scene Name is empty on VillageToCultistExit."
            );
            return;
        }

        isLoading = true;

        Debug.Log("Leaving the village and entering: " + cultistSceneName);

        SceneManager.LoadScene(cultistSceneName, LoadSceneMode.Single);
    }
}