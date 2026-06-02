using UnityEngine;

public class ExplorationPartySpawner : MonoBehaviour
{
    [Header("Paladin")]
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
            Debug.LogWarning("Paladin follower prefab or spawn point is not assigned.");
            return;
        }

        Instantiate(
            paladinFollowerPrefab,
            paladinSpawnPoint.position,
            Quaternion.identity
        );
    }
}