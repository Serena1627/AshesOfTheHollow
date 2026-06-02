using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [Header("Current Battle Party")]
    [Tooltip("Battle prefabs for party members currently available in combat.")]
    [SerializeField] private List<GameObject> currentPartyPrefabs = new List<GameObject>();

    [Header("Recruitable Member Prefabs")]
    [Tooltip("Assign the Paladin battle prefab here, not the village NPC sprite/prefab.")]
    [SerializeField] private GameObject paladinBattlePrefab;

    [Header("Recruitment State")]
    [SerializeField] private bool paladinRecruited;

    public bool PaladinRecruited => paladinRecruited;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Keeps the party list consistent when testing with recruited flags enabled.
        SyncRecruitedMembersToParty();
    }

    /// <summary>
    /// Returns a copy of the current party prefab list.
    /// BattleSceneManager can use this to spawn the available party members.
    /// </summary>
    public List<GameObject> GetCurrentPartyPrefabs()
    {
        return new List<GameObject>(currentPartyPrefabs);
    }

    /// <summary>
    /// Adds a battle prefab to the active party if it is not already included.
    /// </summary>
    public void AddPartyMember(GameObject partyMemberPrefab)
    {
        if (partyMemberPrefab == null)
        {
            Debug.LogWarning("Cannot add a null party member prefab.");
            return;
        }

        if (currentPartyPrefabs.Contains(partyMemberPrefab))
        {
            return;
        }

        currentPartyPrefabs.Add(partyMemberPrefab);
        Debug.Log(partyMemberPrefab.name + " added to the current party.");
    }

    /// <summary>
    /// Removes a battle prefab from the active party.
    /// </summary>
    public void RemovePartyMember(GameObject partyMemberPrefab)
    {
        if (partyMemberPrefab == null)
        {
            return;
        }

        if (currentPartyPrefabs.Remove(partyMemberPrefab))
        {
            Debug.Log(partyMemberPrefab.name + " removed from the current party.");
        }
    }

    /// <summary>
    /// Checks whether a specific battle prefab is already in the active party.
    /// </summary>
    public bool HasPartyMember(GameObject partyMemberPrefab)
    {
        return partyMemberPrefab != null &&
               currentPartyPrefabs.Contains(partyMemberPrefab);
    }

    /// <summary>
    /// Called after the Paladin recruitment dialogue and join announcement.
    /// Adds Paladin's battle prefab to the party.
    /// </summary>
    public void RecruitPaladin()
    {
        if (paladinRecruited)
        {
            Debug.Log("Paladin has already been recruited.");
            return;
        }

        if (paladinBattlePrefab == null)
        {
            Debug.LogWarning("Paladin cannot be recruited because Paladin Battle Prefab is not assigned in PartyManager.");
            return;
        }

        paladinRecruited = true;
        AddPartyMember(paladinBattlePrefab);

        Debug.Log("Paladin has joined the party.");
    }

    /// <summary>
    /// Ensures recruited characters are present in the party list.
    /// Useful when testing recruitment flags through the Inspector.
    /// </summary>
    private void SyncRecruitedMembersToParty()
    {
        if (paladinRecruited && paladinBattlePrefab != null)
        {
            AddPartyMember(paladinBattlePrefab);
        }
    }
}