using UnityEngine;

public class ExplorationPartySpawner : MonoBehaviour
{
    [Header("Paladin Follower")]
    [SerializeField] private GameObject paladinFollowerPrefab;
    [SerializeField] private Transform paladinSpawnPoint;

    private void Start()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("ExplorationPartySpawner could not find PartyManager.");
            return;
        }

        if (!PartyManager.Instance.PaladinRecruited)
        {
            return;
        }

        if (paladinFollowerPrefab == null || paladinSpawnPoint == null)
        {
            Debug.LogWarning("Paladin follower prefab or spawn point is missing.");
            return;
        }

        GameObject follower = Instantiate(
            paladinFollowerPrefab,
            paladinSpawnPoint.position,
            Quaternion.identity
        );

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        PaladinFollowPlayer followScript =
            follower.GetComponent<PaladinFollowPlayer>();

        if (playerObject != null && followScript != null)
        {
            followScript.SetPlayer(playerObject.transform);
        }
    }
}