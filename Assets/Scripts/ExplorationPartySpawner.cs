using System.Collections;
using UnityEngine;

public class ExplorationPartySpawner : MonoBehaviour
{
    [Header("Paladin Follower")]
    [Tooltip("Assign the top-down Paladin follower prefab, not PaladinBattle.")]
    [SerializeField] private GameObject paladinFollowerPrefab;

    [Tooltip("Distance behind Kael where Paladin appears when the scene loads.")]
    [SerializeField] private float paladinSpawnDistance = 1.1f;

    [SerializeField] private bool allowPaladinFollower = true;

    private IEnumerator Start()
    {
        // Allows PlayerBattleReturnPosition to restore Kael's position first
        // when coming back from a battle.
        yield return null;

        SpawnPaladinIfRecruited();
    }

    private void SpawnPaladinIfRecruited()
    {
        if (!allowPaladinFollower)
        {
            return;
        }

        if (PartyManager.Instance == null)
        {
            Debug.LogWarning(
                "ExplorationPartySpawner could not find PartyManager."
            );
            return;
        }

        if (!PartyManager.Instance.PaladinRecruited)
        {
            return;
        }

        if (paladinFollowerPrefab == null)
        {
            Debug.LogWarning(
                "Paladin is recruited, but Paladin Follower Prefab is not assigned."
            );
            return;
        }

        // Prevent two followers if one was already spawned in this scene.
        PaladinFollowPlayer existingFollower =
            FindFirstObjectByType<PaladinFollowPlayer>();

        if (existingFollower != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning(
                "Paladin cannot spawn because no object tagged Player exists."
            );
            return;
        }

        KaelTopDownController kaelController =
            playerObject.GetComponent<KaelTopDownController>();

        if (kaelController == null)
        {
            Debug.LogWarning(
                "Paladin cannot spawn because the Player object does not have KaelTopDownController."
            );
            return;
        }

        Vector3 spawnPosition =
            kaelController.GetFollowerSpawnPosition(paladinSpawnDistance);

        GameObject follower = Instantiate(
            paladinFollowerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        PaladinFollowPlayer followScript =
            follower.GetComponent<PaladinFollowPlayer>();

        if (followScript == null)
        {
            Debug.LogWarning(
                "The Paladin follower prefab does not contain PaladinFollowPlayer."
            );
            Destroy(follower);
            return;
        }

        followScript.SetPlayer(playerObject.transform);

        Debug.Log("Paladin follower spawned behind Kael in this scene.");
    }
}