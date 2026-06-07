using System.Collections;
using UnityEngine;

public class ExplorationPartySpawner : MonoBehaviour
{
    [Header("Paladin Follower")]
    [SerializeField] private GameObject paladinFollowerPrefab;

    [Tooltip("Distance behind Kael where Paladin appears.")]
    [SerializeField] private float paladinSpawnDistance = 1.1f;

    [SerializeField] private bool allowPaladinFollower = true;

    private IEnumerator Start()
    {
        // Allow SceneArrivalPositioner to move Kael first.
        yield return null;
        yield return null;

        if (!allowPaladinFollower)
        {
            yield break;
        }

        if (PartyManager.Instance == null)
        {
            Debug.LogWarning(
                "ExplorationPartySpawner could not find PartyManager."
            );

            yield break;
        }

        if (!PartyManager.Instance.PaladinRecruited)
        {
            yield break;
        }

        if (paladinFollowerPrefab == null)
        {
            Debug.LogWarning(
                "ExplorationPartySpawner is missing Paladin Follower Prefab."
            );

            yield break;
        }

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning(
                "ExplorationPartySpawner could not find Player."
            );

            yield break;
        }

        Vector3 spawnPosition =
            playerObject.transform.position +
            Vector3.down * paladinSpawnDistance;

        GameObject follower = Instantiate(
            paladinFollowerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        PaladinFollowPlayer followScript =
            follower.GetComponent<PaladinFollowPlayer>();

        if (followScript != null)
        {
            followScript.SetPlayer(playerObject.transform);
        }

        Debug.Log(
            "Paladin follower spawned behind Kael at: " +
            spawnPosition
        );
    }
}