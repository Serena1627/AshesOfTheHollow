using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [SerializeField] private List<GameObject> currentPartyPrefabs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<GameObject> GetCurrentPartyPrefabs()
    {
        return new List<GameObject>(currentPartyPrefabs);
    }

    public void AddPartyMember(GameObject partyMemberPrefab)
    {
        if (!currentPartyPrefabs.Contains(partyMemberPrefab))
        {
            currentPartyPrefabs.Add(partyMemberPrefab);
        }
    }

    public void RemovePartyMember(GameObject partyMemberPrefab)
    {
        currentPartyPrefabs.Remove(partyMemberPrefab);
    }
}