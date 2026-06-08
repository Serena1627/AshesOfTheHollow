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

        if (!SceneEntryData.TryConsumeEntryForScene(
                currentSceneName,
                out string requestedSpawnId,
                out string requestedFacing))
        {
            yield break;
        }

        requestedSpawnId = requestedSpawnId.Trim();

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
                "SceneArrivalPositioner could not find spawn ID: " +
                requestedSpawnId
            );

            yield break;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("SceneArrivalPositioner could not find Player.");
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

        KaelTopDownController kael =
            playerObject.GetComponent<KaelTopDownController>();

        if (kael != null)
        {
            ApplyFacing(kael, requestedFacing);
        }

        Debug.Log(
            "Moved Kael to " +
            requestedSpawnId +
            " facing " +
            requestedFacing
        );
    }

    private void ApplyFacing(
        KaelTopDownController kael,
        string facing
    )
    {
        if (string.IsNullOrWhiteSpace(facing))
        {
            return;
        }

        switch (facing.Trim().ToLower())
        {
            case "back":
            case "up":
                kael.FaceBack();
                break;

            case "left":
                kael.FaceLeft();
                break;

            case "right":
                kael.FaceRight();
                break;

            case "front":
            case "down":
                kael.FaceFront();
                break;

            default:
                Debug.LogWarning("Unknown facing direction: " + facing);
                break;
        }
    }
}