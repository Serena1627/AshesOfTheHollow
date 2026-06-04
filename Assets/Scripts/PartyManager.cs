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

    [System.Serializable]
    public class PartyMemberHealthState
    {
        public string memberId;
        public int currentHealth;
        public int maxHealth;
    }

    [Header("Persistent Party Health")]
    [SerializeField]
    private List<PartyMemberHealthState> partyHealthStates =
        new List<PartyMemberHealthState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (GameObject partyMemberPrefab in currentPartyPrefabs)
        {
            RegisterInitialHealthFromPrefab(partyMemberPrefab);
        }

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
            return;
        }

        if (!currentPartyPrefabs.Contains(partyMemberPrefab))
        {
            currentPartyPrefabs.Add(partyMemberPrefab);
        }

        RegisterInitialHealthFromPrefab(partyMemberPrefab);
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
    public void RegisterInitialHealthFromPrefab(GameObject partyMemberPrefab)
    {
        if (partyMemberPrefab == null)
        {
            return;
        }

        PlayerBattle battleMember = partyMemberPrefab.GetComponent<PlayerBattle>();

        if (battleMember == null)
        {
            Debug.LogWarning(
                partyMemberPrefab.name +
                " does not contain PlayerBattle, so its health could not be registered."
            );

            return;
        }

        PartyMemberHealthState existingState =
            GetHealthState(battleMember.entityName);

        if (existingState != null)
        {
            return;
        }

        PartyMemberHealthState newState = new PartyMemberHealthState
        {
            memberId = battleMember.entityName,
            currentHealth = battleMember.health,
            maxHealth = battleMember.health
        };

        partyHealthStates.Add(newState);

        Debug.Log(
            "Registered initial HP for " +
            newState.memberId +
            ": " +
            newState.currentHealth +
            " / " +
            newState.maxHealth
        );
    }

    public void SaveBattleHealth(PlayerBattle battleMember)
    {
        if (battleMember == null)
        {
            return;
        }

        PartyMemberHealthState state =
            GetHealthState(battleMember.entityName);

        if (state == null)
        {
            state = new PartyMemberHealthState
            {
                memberId = battleMember.entityName
            };

            partyHealthStates.Add(state);
        }

        state.currentHealth = battleMember.CurrentHealth;
        state.maxHealth = battleMember.MaxHealth;

        Debug.Log(
            "Saved party HP for " +
            state.memberId +
            ": " +
            state.currentHealth +
            " / " +
            state.maxHealth
        );
    }

    public void ApplyStoredHealth(PlayerBattle battleMember)
    {
        if (battleMember == null)
        {
            return;
        }

        PartyMemberHealthState state =
            GetHealthState(battleMember.entityName);

        if (state == null)
        {
            state = new PartyMemberHealthState
            {
                memberId = battleMember.entityName,
                currentHealth = battleMember.CurrentHealth,
                maxHealth = battleMember.MaxHealth
            };

            partyHealthStates.Add(state);

            return;
        }

        battleMember.RestoreHealthFromPartyState(
            state.currentHealth,
            state.maxHealth
        );
    }

    public bool TryGetPartyHealth(
        string memberId,
        out int currentHealth,
        out int maxHealth
    )
    {
        currentHealth = 0;
        maxHealth = 0;

        PartyMemberHealthState state = GetHealthState(memberId);

        if (state == null)
        {
            return false;
        }

        currentHealth = state.currentHealth;
        maxHealth = state.maxHealth;

        return true;
    }

    private PartyMemberHealthState GetHealthState(string memberId)
    {
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return null;
        }

        return partyHealthStates.Find(
            state => string.Equals(
                state.memberId,
                memberId,
                System.StringComparison.OrdinalIgnoreCase
            )
        );
    }
}