using System.Collections;
using UnityEngine;

public class ExplorationPartySpawner : MonoBehaviour
{
    [Header("Paladin Follower")]
    [SerializeField] private GameObject paladinFollowerPrefab;
    [SerializeField] private float paladinSpawnDistance = 1.1f;

    [Header("Mira Follower")]
    [SerializeField] private GameObject miraFollowerPrefab;
    [SerializeField] private float miraSpawnDistance = 1.1f;

    [Header("Settings")]
    [SerializeField] private bool allowPaladinFollower = true;
    [SerializeField] private bool allowMiraFollower = true;

    private IEnumerator Start()
    {
        // Wait for player positioning scripts to finish first.
        yield return null;
        yield return null;
        yield return null;

        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("ExplorationPartySpawner could not find PartyManager.");
            yield break;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("ExplorationPartySpawner could not find Player.");
            yield break;
        }

        GameObject paladinFollower = null;

        if (allowPaladinFollower && PartyManager.Instance.PaladinRecruited)
        {
            paladinFollower = SpawnFollowerBehindTarget(
                paladinFollowerPrefab,
                playerObject.transform,
                paladinSpawnDistance,
                "PaladinFollower",
                "Paladin"
            );
        }

        if (allowMiraFollower && PartyManager.Instance.MiraRecruited)
        {
            Transform miraTarget = paladinFollower != null
                ? paladinFollower.transform
                : playerObject.transform;

            SpawnFollowerBehindTarget(
                miraFollowerPrefab,
                miraTarget,
                miraSpawnDistance,
                "MiraFollower",
                "Mira"
            );
        }
    }

    private GameObject SpawnFollowerBehindTarget(
        GameObject followerPrefab,
        Transform target,
        float distance,
        string spawnedObjectName,
        string displayName
    )
    {
        if (followerPrefab == null)
        {
            Debug.LogWarning(displayName + " follower prefab is not assigned.");
            return null;
        }

        if (target == null)
        {
            Debug.LogWarning(displayName + " follower target is missing.");
            return null;
        }

        GameObject existingFollower = GameObject.Find(spawnedObjectName);

        if (existingFollower != null)
        {
            Debug.Log(displayName + " follower already exists in this scene.");
            return existingFollower;
        }

        Vector3 spawnPosition = target.position + Vector3.down * distance;

        GameObject follower = Instantiate(
            followerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        follower.name = spawnedObjectName;

        PaladinFollowPlayer followScript =
            follower.GetComponent<PaladinFollowPlayer>();

        if (followScript != null)
        {
            followScript.SetPlayer(target);
        }
        else
        {
            Debug.LogWarning(
                displayName +
                " follower prefab is missing PaladinFollowPlayer."
            );
        }

        Debug.Log(
            displayName +
            " follower spawned behind " +
            target.name +
            " at " +
            spawnPosition
        );

        return follower;
    }
}