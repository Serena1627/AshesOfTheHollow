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
        string currentSceneName = SceneManager.GetActiveScene().name;

        Debug.Log("SceneArrivalPositioner active in scene: " + currentSceneName);

        if (!SceneEntryData.TryConsumeEntryForScene(
                currentSceneName,
                out string requestedSpawnId))
        {
            Debug.Log("No requested spawn ID for scene: " + currentSceneName);
            yield break;
        }

        requestedSpawnId = requestedSpawnId.Trim();

        Debug.Log("Requested spawn ID: [" + requestedSpawnId + "]");
        Debug.Log("Arrival point count: " + arrivalPoints.Count);

        foreach (ArrivalPoint point in arrivalPoints)
        {
            string listedId = point != null && point.spawnId != null
                ? point.spawnId.Trim()
                : "NULL";

            Debug.Log(
                "Available arrival ID: [" +
                listedId +
                "] | Transform: " +
                (point != null && point.spawnPoint != null
                    ? point.spawnPoint.name
                    : "NULL")
            );
        }

        yield return null;
        yield return null;

        ArrivalPoint selectedArrival = arrivalPoints.Find(
            point => point != null &&
                     point.spawnPoint != null &&
                     !string.IsNullOrWhiteSpace(point.spawnId) &&
                     point.spawnId.Trim() == requestedSpawnId
        );

        if (selectedArrival == null)
        {
            Debug.LogWarning(
                "SceneArrivalPositioner could not find spawn ID: [" +
                requestedSpawnId +
                "]"
            );

            yield break;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("Could not find Player object.");
            yield break;
        }

        Vector3 destination = selectedArrival.spawnPoint.position;

        Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.position = new Vector2(destination.x, destination.y);
            rb.linearVelocity = Vector2.zero;
        }

        playerObject.transform.position = destination;

        Debug.Log(
            "Moved Kael to " +
            requestedSpawnId +
            " at position " +
            destination
        );
    }
}