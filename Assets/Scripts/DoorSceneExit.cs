using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorSceneExit : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "VillageScene";

    private bool isLoading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Door trigger entered by: " + other.name);

        TryLoadNextScene(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryLoadNextScene(other);
    }

    private void TryLoadNextScene(Collider2D other)
    {
        if (isLoading)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Object touching door is not tagged Player: " + other.name);
            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("Next Scene Name is empty on DoorSceneExit.");
            return;
        }

        Debug.Log("Loading next scene: " + nextSceneName);

        isLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }
}