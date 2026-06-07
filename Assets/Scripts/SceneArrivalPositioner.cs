using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneArrivalPositioner : MonoBehaviour
{
    [System.Serializable]
    public class ArrivalPoint
    {
        public string spawnId;
        public Transform spawnPoint;
    }

    [Header("Arrival Points In This Scene")]
    [SerializeField] private List<ArrivalPoint> arrivalPoints =
        new List<ArrivalPoint>();

    private IEnumerator Start()
    {
        string currentSceneName =
            SceneManager.GetActiveScene().name;

        if (!SceneEntryData.TryConsumeEntryForScene(
            currentSceneName,
            out string requestedSpawnId
        ))
        {
            Debug.Log(
                "No requested arrival position for scene: " +
                currentSceneName
            );

            yield break;
        }

        // Wait for Player and other scene-start scripts to finish initialization.
        yield return null;

        ArrivalPoint selectedArrival = arrivalPoints.Find(
            point => point != null &&
                     point.spawnId == requestedSpawnId
        );

        if (selectedArrival == null ||
            selectedArrival.spawnPoint == null)
        {
            Debug.LogWarning(
                "SceneArrivalPositioner could not find spawn ID: " +
                requestedSpawnId
            );

            yield break;
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning(
                "SceneArrivalPositioner could not find the Player object."
            );

            yield break;
        }

        Vector3 destination =
            selectedArrival.spawnPoint.position;

        Rigidbody2D playerRigidbody =
            playerObject.GetComponent<Rigidbody2D>();

        if (playerRigidbody != null)
        {
            playerRigidbody.position =
                new Vector2(destination.x, destination.y);
        }

        playerObject.transform.position = destination;

        Debug.Log(
            "Moved Kael to arrival point: " +
            requestedSpawnId +
            " at " +
            destination
        );
    }
}